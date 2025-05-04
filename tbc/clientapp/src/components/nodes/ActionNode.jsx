// clientapp/src/components/nodes/ActionNode.jsx
import React from 'react';
import { Handle, Position } from 'reactflow';
import './NodeStyles.css';

export default function ActionNode({ id, data }) {
    return (
        <div className="custom-node action-node">
            <div className="node-header">
                <span>🔔 {data.label || 'Actions'}</span>
                <div className="node-actions">
                    <button onClick={() => data.onEdit(id)}>✎</button>
                    <button onClick={() => data.onDelete(id)}>✕</button>
                </div>
            </div>

            {/* круглый вход слева */}
            <Handle
                type="target"
                id="in"
                position={Position.Left}
                style={{ background: '#555' }}
            />

            {/* прямоугольный магнетический выход снизу */}
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
