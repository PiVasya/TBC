// clientapp/src/components/nodes/ActionNode.jsx
import React from 'react';
import { Handle, Position } from 'reactflow';
import './NodeStyles.css';

export default function ActionNode({ id, data }) {
    return (
        <div className="custom-node action-node">
            <div className="node-header">
                <span>🔔 Actions</span>
                <div className="node-actions">
                    <button onClick={() => data.onEdit(id)}>✎</button>
                    <button onClick={() => data.onDelete(id)}>✕</button>
                </div>
            </div>
            {/* вход слева */}
            <Handle
                type="target"
                position={Position.Left}
                id="left-in"
                style={{ background: '#555' }}
            />
            {/* два выхода снизу */}
            <Handle
                type="source"
                position={Position.Bottom}
                id="out-1"
                style={{ background: '#555', left: '25%' }}
            />
            <Handle
                type="source"
                position={Position.Bottom}
                id="out-2"
                style={{ background: '#555', right: '25%' }}
            />
        </div>
    );
}
