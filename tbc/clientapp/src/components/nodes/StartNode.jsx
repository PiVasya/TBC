// clientapp/src/components/nodes/StartNode.jsx
import React from 'react';
import { Handle, Position } from 'reactflow';
import './NodeStyles.css';

export default function StartNode({ id, data }) {
    return (
        <div className="custom-node start-node">
            <div className="node-header">
                <span>▶️ Start</span>
                <div className="node-actions">
                    <button onClick={() => data.onEdit(id)}>✎</button>
                    <button onClick={() => data.onDelete(id)}>✕</button>
                </div>
            </div>
            {/* единственный выход справа */}
            <Handle
                type="source"
                position={Position.Right}
                id="out"
                style={{ background: '#555' }}
            />
        </div>
    );
}
