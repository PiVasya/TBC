// src/components/nodes/StartNode.jsx
import React, { useState } from 'react';
import { Handle, Position } from 'reactflow';
import InlineNodeEditor from '../InlineNodeEditor';
import './NodeStyles.css';

export default function StartNode({ id, data }) {
    const [editing, setEditing] = useState(false);

    const schemaFields = [
        { name: 'command', label: 'Команда запуска', type: 'text' },
    ];

    const handleSave = upd => {
        data.onSave(id, upd);
        setEditing(false);
    };

    return (
        <div className="custom-node start-node" style={{ position: 'relative' }}>
            <div className="node-header">
                <span>▶️ {data.command || 'Start'}</span>
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
                type="source"
                position={Position.Right}
                id="out"
                style={{ background: '#555' }}
            />
        </div>
    );
}
