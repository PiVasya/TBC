// clientapp/src/components/nodes/ButtonNode.jsx
import React from 'react';
import { Handle, Position } from 'reactflow';
import './NodeStyles.css';

export default function ButtonNode({ id, data }) {
    return (
        <div className="custom-node button-node">
            <div className="node-header">
                <span>🔘 Button</span>
                <div className="node-actions">
                    <button onClick={() => data.onEdit(id)}>✎</button>
                    <button onClick={() => data.onDelete(id)}>✕</button>
                </div>
            </div>
            {/* Вход сверху */}
            <Handle
                type="target"
                position={Position.Top}
                id="in"
                style={{ background: '#555' }}
            />
            {/* Выход снизу */}
            <Handle
                type="source"
                position={Position.Bottom}
                id="out"
                style={{ background: '#555' }}
            />
        </div>
    );
}
