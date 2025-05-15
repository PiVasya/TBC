// clientapp/src/components/nodes/ButtonNode.jsx
import React, { useState } from 'react';
import { Handle, Position } from 'reactflow';
import InlineNodeEditor from '../InlineNodeEditor';
import './NodeStyles.css';

export default function ButtonNode({ id, data }) {
    const [editing, setEditing] = useState(false);

    // Поля для редактирования
    const schemaFields = [
        { name: 'label', type: 'text', label: 'Текст на кнопке' },
        {
            name: 'placement', type: 'select', label: 'Расположение', options: [
                { value: 'inline', label: 'В строке под текстом' },
                { value: 'block', label: 'Отдельным сообщением' }
            ]
        },
        { name: 'saveToDb', type: 'checkbox', label: 'Сохранять в БД' },
    ];

    const handleSave = upd => {
        data.onSave(id, upd);
        setEditing(false);
    };

    return (
        <div className="custom-node button-node" style={{ position: 'relative' }}>
            <div className="node-header">
                {/* Только значок или статичная надпись */}
                <span>🔘</span>
                <div className="node-actions">
                    <button onClick={() => setEditing(true)}>✎</button>
                    <button onClick={() => data.onDelete(id)}>✕</button>
                </div>
            </div>

            {/* Текст кнопки отображаем только здесь */}
            <div className={
                data.placement === 'inline'
                    ? 'button-preview inline'
                    : 'button-preview block'
            }>
                {data.label || 'Button'}
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
                id="top"
                position={Position.Top}
                className="rect-handle"
            />
            <Handle
                type="source"
                id="bottom"
                position={Position.Bottom}
                className="rect-handle"
            />
            <Handle
                type="source"
                id="right"
                position={Position.Right}
                style={{ background: '#555' }}
            />
        </div>
    );
}
