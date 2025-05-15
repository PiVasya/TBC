// clientapp/src/components/nodes/DelayNode.jsx
import React, { useState } from 'react';
import { Handle, Position } from 'reactflow';
import InlineNodeEditor from '../InlineNodeEditor';
import './NodeStyles.css';

export default function DelayNode({ id, data }) {
    const [editing, setEditing] = useState(false);

    const schemaFields = [
        { name: 'duration', label: 'Задержка (сек)', type: 'text' },
        { name: 'saveToDb', label: 'Сохранять в БД', type: 'checkbox' },
        { name: 'notifyAdmin', label: 'Уведомить админа', type: 'checkbox' },
    ];

    const handleSave = upd => {
        data.onSave(id, upd);
        setEditing(false);
    };

    return (
        <div className="custom-node delay-node" style={{ position: 'relative' }}>
            <div className="node-header">
                <span>⏳ {data.duration ? `${data.duration}s` : 'Delay'}</span>
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
