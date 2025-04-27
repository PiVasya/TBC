// clientapp/src/components/nodes/TextNode.jsx
import React from 'react';
import { Handle, Position } from 'reactflow';
import './NodeStyles.css';

export default function TextNode({ id, data }) {
    return (
        <div className="custom-node text-node">
            <div className="node-header">
                <span>📝 {data.label || 'Text'}</span>
                <div className="node-actions">
                    <button onClick={() => data.onEdit(id)}>✎</button>
                    <button onClick={() => data.onDelete(id)}>✕</button>
                </div>
            </div>
            {/* вход слева, выход справа */}
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
