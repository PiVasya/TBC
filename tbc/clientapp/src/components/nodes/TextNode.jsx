// src/components/nodes/TextNode.jsx
import React, { useState } from 'react';
import { Handle, Position } from 'reactflow';
import InlineNodeEditor from '../InlineNodeEditor';
import './NodeStyles.css';

export default function TextNode({ id, data }) {
    const [editing, setEditing] = useState(false);

    const schemaFields = [
        { name: 'label', label: 'Текст ноды', type: 'textarea' },
        { name: 'saveToDb', label: 'Сохранять в БД', type: 'checkbox' },
        { name: 'notifyAdmin', label: 'Уведомить админа', type: 'checkbox' },
        { name: 'delete', label: 'Удалить сообщение', type: 'checkbox' },
        { name: 'duration', label: 'Задержка удаления(сек)', type: 'text' },
        { name: 'logUsage', label: 'Логировать использование', type: 'checkbox' },
    ];

    const handleSave = upd => {
        data.onSave(id, upd);
        setEditing(false);
    };

    return (
        <div className="custom-node text-node" style={{ position: 'relative' }}>
            <div className="node-header">
                <span>📝 {data.label || 'Text'}</span>
                <div className="node-actions">
                    <button onClick={() => setEditing(true)}>✎</button>
                    <button onClick={() => data.onDelete(id)}>✕</button>
                </div>
            </div>

            {editing && (
                <InlineNodeEditor
                    data={data}
                    schemaFields={schemaFields}
                    onSave={handleSave}
                    onCancel={() => setEditing(false)}
                />
            )}

            <Handle
                type="target"
                position={Position.Left}
                id="in"
                style={{ background: '#555' }}
            />
            <Handle
                type="source"
                position={Position.Right}
                id="out"
                style={{ background: '#555' }}
            />
        </div>
    );
}
