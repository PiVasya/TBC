// clientapp/src/components/DockerMenu.jsx
import React from 'react';
import './DockerMenu.css';

export default function DockerMenu({ items, onToggle, onDelete }) {
    return (
        <div className="docker-menu">
            {items.map(c => (
                <div key={c.id} className="docker-menu-item card">
                    <div className="info">
                        <strong>{c.name}</strong>
                        <code>({c.id.slice(0, 12)})</code>
                        <div className="sub">Image: {c.image}</div>
                    </div>
                    <div className="actions">
                        <button
                            className={`app-button sm ${c.status === 'running' ? '' : 'outline'}`}
                            onClick={() => onToggle(c)}
                        >
                            {c.status === 'running' ? 'Остановить' : 'Запустить'}
                        </button>
                        <button
                            className="app-button sm danger outline"
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
