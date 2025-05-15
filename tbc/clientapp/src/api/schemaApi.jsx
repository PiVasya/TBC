// clientapp/src/api/schemaApi.js

const base = botId => `/api/bots/${botId}/schemas`;

/** Список сохранённых версий схем */
export async function listSchemas(botId) {
    const res = await fetch(base(botId));
    if (!res.ok) throw new Error(res.statusText);
    return res.json();
}

/** Загрузить одну версию (по её id) */
export async function getSchema(botId, schemaId) {
    const res = await fetch(`${base(botId)}/${schemaId}`);
    if (!res.ok) throw new Error(res.statusText);
    return res.json();
}

/** Сохранить новую версию схемы */
export async function postSchema(botId, schema) {
    const res = await fetch(base(botId), {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(schema),
    });
    if (!res.ok) {
        const err = await res.json();
        throw new Error(err.error || res.statusText);
    }
    return res.json(); // вернёт { id, createdAt }
}
