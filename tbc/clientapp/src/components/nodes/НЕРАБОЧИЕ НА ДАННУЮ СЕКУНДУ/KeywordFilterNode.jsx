// clientapp/src/components/nodes/KeywordFilterNode.jsx
/*import React, { useState } from 'react';
import { Handle, Position } from 'reactflow';
import InlineNodeEditor from '../InlineNodeEditor';
import './NodeStyles.css';

export default function KeywordFilterNode({ id, data }) {
    const [editing, setEditing] = useState(false);

    const schemaFields = [
        {
            name: 'keywords',
            label: 'Слова (через запятую)',
            type: 'textarea',
            rows: 2
        },
        { name: 'caseSensitive', label: 'Учитывать регистр', type: 'checkbox' },
        { name: 'saveToDb', label: 'Сохранять в БД', type: 'checkbox' },
        { name: 'notifyAdmin', label: 'Уведомить админа', type: 'checkbox' },
        { name: 'logUsage', label: 'Логировать срабатывание', type: 'checkbox' },
    ];

    const handleSave = upd => {
        data.onSave(id, upd);
        setEditing(false);
    };

    return (
        <div className="custom-node keyword-filter-node" style={{ position: 'relative' }}>
            <div className="node-header">
                <span>🔍 Фильтр слов</span>
                <div className="node-actions">
                    <button onClick={() => setEditing(true)}>✎</button>
                    <button onClick={() => data.onDelete(id)}>✕</button>
                </div>
            </div>
            <div className="filter-preview">
                {data.keywords
                    ? data.keywords.split(',')
                        .map(w => w.trim())
                        .filter(w => w)
                        .slice(0, 5)
                        .join(', ')
                    : <i>– нет слов –</i>
                }
                {data.keywords && data.keywords.split(',').length > 5 && '…'}
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
                id="out"
                position={Position.Right}
                style={{ background: '#555' }}
            />
        </div>
    );
}
*/