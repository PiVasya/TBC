// clientapp/src/components/nodes/ActionNode.jsx
import React, { useState } from 'react';
import { Handle, Position } from 'reactflow';
import InlineNodeEditor from '../InlineNodeEditor';
import './NodeStyles.css';

export default function ActionNode({ id, data }) {
    const [editing, setEditing] = useState(false);

    // Описание полей, которые можно редактировать
    const schemaFields = [
        {
            name: 'label',
            label: 'Название действия',
            type: 'text'
        },
        {
            name: 'saveToDb',
            label: 'Сохранять ответ в БД',
            type: 'checkbox'
        },
        {
            name: 'notifyAdmin',
            label: 'Уведомить админа',
            type: 'checkbox'
        }
    ];

    // Сохраняем изменения через data.onSave и закрываем панель
    const handleSave = upd => {
        data.onSave(id, upd);
        setEditing(false);
    };

    return (
        <div className="custom-node action-node" style={{ position: 'relative' }}>
            <div className="node-header">
                <span>🔔 {data.label || 'Actions'}</span>
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
                id="in"
                position={Position.Left}
                style={{ background: '#555' }}
            />
            <Handle
                type="source"
                id="bottom"
                position={Position.Bottom}
                className="rect-handle"
                isConnectable={false}
            />
        </div>
    );
}
