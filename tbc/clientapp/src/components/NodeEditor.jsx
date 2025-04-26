// clientapp/src/components/NodeEditor.jsx
import React, { useCallback } from 'react';
import ReactFlow, {
    addEdge,
    Background,
    Controls,
    useNodesState,
    useEdgesState
} from 'reactflow';
import 'reactflow/dist/style.css';

export default function NodeEditor() {
    const [nodes, setNodes, onNodesChange] = useNodesState([]);
    const [edges, setEdges, onEdgesChange] = useEdgesState([]);

    const onConnect = useCallback(
        params => setEdges(eds => addEdge(params, eds)),
        [setEdges]
    );

    const addNode = useCallback(() => {
        const id = (nodes.length + 1).toString();
        const offset = 100;
        const newNode = {
            id,
            data: { label: `Узел ${id}` },
            position: {
                x: offset * nodes.length + 50,
                y: offset * nodes.length + 50
            },
        };
        setNodes(nds => [...nds, newNode]);
    }, [nodes, setNodes]);

    return (
        <div className="node-editor-container">
            <button className="add-node-button" onClick={addNode}>
                Добавить узел
            </button>
            <div style={{ width: '100%', height: 600, border: '1px solid #ddd' }}>
                <ReactFlow
                    nodes={nodes}
                    edges={edges}
                    onNodesChange={onNodesChange}
                    onEdgesChange={onEdgesChange}
                    onConnect={onConnect}
                    fitView
                >
                    <Background gap={16} />
                    <Controls />
                </ReactFlow>
            </div>
        </div>
    );
}
