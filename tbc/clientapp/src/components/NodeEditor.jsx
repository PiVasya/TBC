// clientapp/src/components/NodeEditor.jsx
import React, { useCallback, useRef } from 'react';
import ReactFlow, {
    addEdge,
    Background,
    Controls,
    useNodesState,
    useEdgesState,
    MarkerType
} from 'reactflow';
import 'reactflow/dist/style.css';
import './NodeEditor.css';

const req = require.context('./nodes', false, /\.jsx$/);
const nodeTypes = req.keys().reduce((acc, path) => {
    const mod = req(path);
    const name = path.replace('./', '').replace('.jsx', '');
    acc[name] = mod.default;
    return acc;
}, {});

export default function NodeEditor({ onBack }) {
    const idCounter = useRef(1);
    const [nodes, setNodes, onNodesChange] = useNodesState([]);
    const [edges, setEdges, onEdgesChange] = useEdgesState([]);

    const onConnect = useCallback(
        params => {
            if (edges.some(e => e.source === params.source) ||
                edges.some(e => e.target === params.target)) {
                console.warn('Узел уже имеет вход или выход');
                return;
            }
            setEdges(es =>
                addEdge({
                    ...params,
                    markerEnd: { type: MarkerType.ArrowClosed },
                }, es)
            );
        },
        [edges, setEdges]
    );

    const addNode = useCallback(
        type => {
            const id = String(idCounter.current++);
            const offset = 150;
            setNodes(nds => [
                ...nds,
                {
                    id,
                    type: type || Object.keys(nodeTypes)[0],
                    data: {
                        label: `${type || Object.keys(nodeTypes)[0]} ${id}`,
                        onDelete: nid => {
                            setNodes(n => n.filter(x => x.id !== nid));
                            setEdges(e => e.filter(x => x.source !== nid && x.target !== nid));
                        },
                        onEdit: nid => console.log('edit', nid)
                    },
                    position: {
                        x: (nds.length % 3) * offset + 50,
                        y: Math.floor(nds.length / 3) * offset + 50
                    }
                }
            ]);
        },
        [setNodes, setEdges]
    );

    return (
        <div className="node-editor-container">
            <div className="sidebar">
                <button className="app-button outline sm back-btn" onClick={onBack}>
                    ← Вернуться
                </button>
                <h3>Типы нод</h3>
                {Object.keys(nodeTypes).map(type => (
                    <button
                        key={type}
                        className="app-button sm"
                        onClick={() => addNode(type)}
                    >
                        {type}
                    </button>
                ))}
            </div>

            <div className="canvas-area">
                <div className="toolbar">
                    <button className="app-button sm" onClick={() => addNode()}>
                        Добавить узел
                    </button>
                </div>
                <div className="flow-wrapper">
                    <ReactFlow
                        nodes={nodes}
                        edges={edges}
                        onNodesChange={onNodesChange}
                        onEdgesChange={onEdgesChange}
                        onConnect={onConnect}
                        nodeTypes={nodeTypes}
                        fitView
                        snapToGrid
                        snapGrid={[15, 15]}
                    >
                        <Background gap={16} />
                        <Controls />
                    </ReactFlow>
                </div>
                <h3>Текущая структура</h3>
                <pre className="structure-output">
                    {JSON.stringify({ nodes, edges }, null, 2)}
                </pre>
            </div>
        </div>
    );
}
