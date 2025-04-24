import React from 'react';

export default function DockerMenu({ items }) {
    return (
        <div className="docker-menu">
            {items.map(c => (
                <div key={c.id} className="docker-menu-item">
                    {c.name} <code>({c.id})</code>
                </div>
            ))}
        </div>
    );
}
