// clientapp/src/components/DockerMenu.jsx
import React from 'react';

// clientapp/src/components/DockerMenu.jsx
export default function DockerMenu({ items, onToggle, onDelete }) {
    return (
        <div className="docker-menu">
            {items.map(c => (
                <div key={c.id} className="docker-menu-item">
                    <div>
                        <strong>{c.name}</strong> <code>({c.id.slice(0, 12)})</code>
                        <div style={{ fontSize: '0.85em', color: '#666' }}>
                            Image: {c.image}
                        </div>
                    </div>
                    <div className="actions">
                        <button
                            className={`toggle-btn ${c.status === 'running' ? 'stop' : 'start'}`}
                            onClick={() => onToggle(c)}
                        >
                            {c.status === 'running' ? 'Остановить' : 'Запустить'}
                        </button>
                        <button
                            className="delete-btn"
                            onClick={() => onDelete(c)}
                        >
                            Удалить
                        </button>
                    </div>
                </div>
            ))}
        </div>
    );
}

