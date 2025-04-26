// clientapp/src/components/CustomNode.jsx
import React from 'react';
import { Handle, Position } from 'reactflow';
import './CustomNode.css';

export default function CustomNode({ id, data }) {
    return (
        <div className="custom-node">
            <button
                className="node-delete-btn"
                onClick={() => data.onDelete(id)}
            >
                ×
            </button>
            <div className="node-label">{data.label}</div>
            <Handle
                type="target"
                position={Position.Left}
                style={{ top: '50%', background: '#555' }}
            />
            <Handle
                type="source"
                position={Position.Right}
                style={{ top: '50%', background: '#555' }}
            />
        </div>
    );
}
