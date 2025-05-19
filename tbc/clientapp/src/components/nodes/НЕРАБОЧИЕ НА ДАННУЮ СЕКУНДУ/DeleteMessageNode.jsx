// clientapp/src/components/nodes/DeleteMessageNode.jsx
/*import React, { useState } from 'react';
import { Handle, Position } from 'reactflow';
import InlineNodeEditor from '../InlineNodeEditor';
import './NodeStyles.css';

export default function DeleteMessageNode({ id, data }) {
    const [editing, setEditing] = useState(false);

    const schemaFields = [
        { name: 'confirmationText', label: 'Текст подтверждения', type: 'textarea' },
        { name: 'saveToDb', label: 'Сохранять в БД', type: 'checkbox' },
        { name: 'notifyAdmin', label: 'Уведомить админа', type: 'checkbox' },
        { name: 'logUsage', label: 'Логировать удаление', type: 'checkbox' },
    ];

    const handleSave = upd => {
        data.onSave(id, upd);
        setEditing(false);
    };

    return (
        <div className="custom-node delete-message-node" style={{ position: 'relative' }}>
            <div className="node-header">
                <span>🗑️ {data.confirmationText || 'Удалить сообщение'}</span>
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
                id="out"
                position={Position.Right}
                style={{ background: '#555' }}
            />
        </div>
    );
}
*/