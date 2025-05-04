// clientapp/src/components/nodes/ButtonNode.jsx
import React from 'react';
import { Handle, Position } from 'reactflow';
import './NodeStyles.css';

export default function ButtonNode({ id, data }) {
    return (
        <div className="custom-node button-node">
            <div className="node-header">
                <span>🔘 {data.label || 'Button'}</span>
                <div className="node-actions">
                    <button onClick={() => data.onEdit(id)}>✎</button>
                    <button onClick={() => data.onDelete(id)}>✕</button>
                </div>
            </div>

            {/* прямоугольник сверху */}
            <Handle
                type="target"
                id="top"
                position={Position.Top}
                className="rect-handle"
            />

            {/* прямоугольник снизу (точка «магнит») */}
            <Handle
                type="source"
                id="bottom"
                position={Position.Bottom}
                className="rect-handle"
            />

            {/* КРУГЛЫЙ основной выход справа */}
            <Handle
                type="source"
                id="right"
                position={Position.Right}
                style={{ background: '#555' }}
            />
        </div>
    );
}
