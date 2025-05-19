using Microsoft.EntityFrameworkCore;
using tbc.Models.Entities;

namespace tbc.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        { }

        /* ─── Таблицы ───────────────────────────────────────────────────────────── */
        public DbSet<BotInstance> BotInstances { get; set; } = null!;
        public DbSet<BotMessage> BotMessages { get; set; } = null!;
        public DbSet<PollSession> PollSessions { get; set; } = null!;
        public DbSet<PollItem> PollItems { get; set; } = null!;
        public DbSet<BotSchema> BotSchemas { get; set; } = null!;

        /* ─── Конфигурация схемы ────────────────────────────────────────────────── */
        protected override void OnModelCreating(ModelBuilder mb)
        {
            /* ── BotInstance ↔ BotMessage ─────────────────────────────── */
            mb.Entity<BotInstance>()
              .HasMany(b => b.Messages)
              .WithOne(m => m.Bot)
              .HasForeignKey(m => m.BotId);

            /* ── BotInstance ↔ PollSession ────────────────────────────── */
            mb.Entity<BotInstance>()
              .HasMany(b => b.PollSessions)
              .WithOne(s => s.Bot)
              .HasForeignKey(s => s.BotId);

            /* ── BotMessage ───────────────────────────────────────────── */
            mb.Entity<BotMessage>(cfg =>
            {
                cfg.Property(m => m.Timestamp)
                   .HasDefaultValueSql("NOW()");

                cfg.Property(m => m.Content)
                   .IsRequired()                       // text NOT NULL
                   .HasColumnType("text");            // Postgres text

                /* новые поля */
                cfg.Property(m => m.NodeType)
                   .IsRequired()
                   .HasMaxLength(32);                 // varchar(32)

                cfg.Property(m => m.Payload)
                   .HasColumnType("text");            // может быть NULL

                /* полезные индексы */
                cfg.HasIndex(m => m.Timestamp);
                cfg.HasIndex(m => new { m.BotId, m.NodeType });
            });

            /* ── PollSession ─────────────────────────────────────────── */
            mb.Entity<PollSession>(p =>
            {
                p.HasKey(x => x.Id);

                p.HasOne(x => x.Bot)
                 .WithMany(b => b.PollSessions)
                 .HasForeignKey(x => x.BotId);

                p.Property(x => x.StartedAt)
                 .HasDefaultValueSql("NOW()");
            });

            /* ── PollItem ─────────────────────────────────────────────── */
            mb.Entity<PollItem>(i =>
            {
                i.HasKey(x => x.Id);

                i.HasOne(x => x.PollSession)
                 .WithMany(s => s.Items)
                 .HasForeignKey(x => x.PollSessionId);

                i.HasOne(x => x.BotMessage)
                 .WithMany()
                 .HasForeignKey(x => x.BotMessageId);

                i.Property(x => x.Prompt).IsRequired();
                i.Property(x => x.Response).IsRequired();

                i.Property(x => x.RespondedAt)
                 .HasDefaultValueSql("NOW()");
            });

            /* ── BotSchema (JSON схемы) ──────────────────────────────── */
            mb.Entity<BotSchema>(b =>
            {
                b.Property(x => x.Content)
                 .HasColumnType("jsonb");

                b.Property(x => x.CreatedAt)
                 .HasDefaultValueSql("NOW()");

                b.HasOne(x => x.BotInstance)
                 .WithMany(bi => bi.Schemas)
                 .HasForeignKey(x => x.BotInstanceId)
                 .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
