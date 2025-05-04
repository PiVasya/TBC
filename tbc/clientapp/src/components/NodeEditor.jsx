// clientapp/src/components/NodeEditor.jsx
import React, { useCallback, useRef, useState } from 'react';
import ReactFlow, {
    ReactFlowProvider,
    addEdge,
    applyEdgeChanges,
    Background,
    Controls,
    useNodesState,
    useEdgesState,
    MarkerType
} from 'reactflow';
import 'reactflow/dist/style.css';
import './NodeEditor.css';

// Динамический импорт всех типов нод
const req = require.context('./nodes', false, /\.jsx$/);
const nodeTypes = req.keys().reduce((acc, path) => {
    const mod = req(path);
    const name = path.replace('./', '').replace('.jsx', '');
    acc[name] = mod.default;
    return acc;
}, {});

export default function NodeEditor({ onBack }) {
    const idRef = useRef(1);
    const [nodes, setNodes, onNodesChange] = useNodesState([]);
    const [edges, setEdges, onEdgesChange] = useEdgesState([]);
    const [schemaText, setSchemaText] = useState('');

    const loadSchema = () => {
        try {
            const { nodes: loadedNodes, edges: loadedEdges } = JSON.parse(schemaText);
            setNodes(loadedNodes || []);
            setEdges(loadedEdges || []);
            // сбросим счётчик id, чтобы новые ноды не пересеклись
            idRef.current = (loadedNodes?.length || 0) + 1;
        } catch (err) {
            alert('Невалидный JSON: ' + err.message);
        }
    };

    // Блокировка удаления магнитных связей
    const handleEdgesChange = useCallback((changes) => {
        const filtered = changes.filter(change => {
            if (change.type === 'remove') {
                const edge = edges.find(e => e.id === change.id);
                return !edge?.data?.magnet;
            }
            return true;
        });
        setEdges(es => applyEdgeChanges(filtered, es));
    }, [edges, setEdges]);

    // Ручное соединение
    const onConnect = useCallback((params) => {
        setEdges(es => addEdge({
            ...params,
            markerEnd: { type: MarkerType.ArrowClosed },
            data: { magnet: false }
        }, es));
    }, [setEdges]);

    // после onConnect и before onNodeDragStop
    const onEdgeDoubleClick = useCallback((event, edge) => {
        // не даём удалить «магнитные» связи
        if (!edge.data?.magnet) {
            setEdges(es => es.filter(e => e.id !== edge.id));
        }
    }, [setEdges]);

    // Обработка перетаскивания нод
    const onNodeDragStop = useCallback((_, dragged) => {
        // Если ButtonNode уже приклеен — возвращаем на место
        if (dragged.type === 'ButtonNode') {
            const magEdge = edges.find(e => e.data?.magnet && e.target === dragged.id);
            if (magEdge) {
                const srcNode = nodes.find(n => n.id === magEdge.source);
                if (srcNode) {
                    setNodes(nds => nds.map(n =>
                        n.id === dragged.id
                            ? { ...n, position: { x: srcNode.position.x, y: srcNode.position.y + (srcNode.height || dragged.height || 40) } }
                            : n
                    ));
                }
                return;
            }
        }

        // Примагничивание новых ButtonNode
        if (dragged.type === 'ButtonNode') {
            const SNAP = 30;
            const w = dragged.width || 120;
            const h = dragged.height || 40;

            nodes.forEach(target => {
                if (target.id === dragged.id) return;
                if (target.type !== 'ButtonNode' && target.type !== 'ActionNode') return;

                const tx = target.position.x + (target.width || w) / 2;
                const ty = target.position.y + (target.height || h);
                const dx = dragged.position.x + w / 2;
                const dy = dragged.position.y;

                if (Math.abs(tx - dx) < SNAP && Math.abs(ty - dy) < SNAP) {
                    setNodes(nds => nds.map(n =>
                        n.id === dragged.id
                            ? { ...n, position: { x: target.position.x, y: target.position.y + (target.height || h) } }
                            : n
                    ));
                    setEdges(es => addEdge({
                        id: `mag-${target.id}-${dragged.id}`,
                        source: target.id,
                        target: dragged.id,
                        markerEnd: { type: MarkerType.ArrowClosed },
                        data: { magnet: true }
                    }, es));
                }
            });
        }

        // Перемещение ActionNode и всех его приклеенных потомков
        if (dragged.type === 'ActionNode') {
            const adj = {};
            edges.forEach(e => {
                if (e.data?.magnet) {
                    adj[e.source] = adj[e.source] || [];
                    adj[e.source].push(e.target);
                }
            });

            const newPos = {};
            newPos[dragged.id] = { x: dragged.position.x, y: dragged.position.y };
            const compute = (parentId) => {
                const children = adj[parentId] || [];
                const parent = newPos[parentId] || nodes.find(n => n.id === parentId).position;
                const parentHeight = nodes.find(n => n.id === parentId)?.height || 40;
                children.forEach(childId => {
                    newPos[childId] = { x: parent.x, y: parent.y + parentHeight };
                    compute(childId);
                });
            };
            compute(dragged.id);

            setNodes(nds => nds.map(n => newPos[n.id] ? { ...n, position: newPos[n.id] } : n));
        }
    }, [nodes, edges, setNodes, setEdges]);

    // Добавление новой ноды
    const addNode = useCallback((type) => {
        const id = String(idRef.current++);
        const offset = 150;
        setNodes(nds => [...nds, {
            id,
            type,
            data: {
                label: `${type} ${id}`,
                onDelete: nid => {
                    setNodes(n => n.filter(x => x.id !== nid));
                    setEdges(e => e.filter(x => x.source !== nid && x.target !== nid));
                },
                onEdit: nid => console.log('edit', nid)
            },
            position: { x: (nds.length % 3) * offset + 50, y: Math.floor(nds.length / 3) * offset + 50 }
        }]);
    }, [setNodes, setEdges]);

    return (
        <div className="node-editor-container">
            <div className="sidebar">


                <h3>Загрузка схемы</h3>
                <textarea
                    className="form-textarea"
                    style={{ height: '100px', fontSize: '0.75rem' }}
                    placeholder='Вставьте JSON { "nodes": [...], "edges": [...] }'
                    value={schemaText}
                    onChange={e => setSchemaText(e.target.value)}
                />
                <button
                    className="app-button sm"
                    onClick={loadSchema}
                >
                    Загрузить
                </button>


                <button className="app-button outline sm back-btn" onClick={onBack}>← Вернуться</button>
                <h3>Типы нод</h3>
                {Object.keys(nodeTypes).map(type => (
                    <button key={type} className="app-button sm" onClick={() => addNode(type)}>{type}</button>
                ))}
            </div>
            <div className="canvas-area">
                <ReactFlowProvider>
                    <ReactFlow
                        nodes={nodes}
                        edges={edges}
                        onNodesChange={onNodesChange}
                        onEdgesChange={handleEdgesChange}
                        onConnect={onConnect}
                        onEdgeDoubleClick={onEdgeDoubleClick}
                        onNodeDragStop={onNodeDragStop}
                        nodeTypes={nodeTypes}
                        fitView
                    >
                        <Background gap={16} />
                        <Controls />
                    </ReactFlow>
                </ReactFlowProvider>
                <h3>Текущая структура</h3>
                <pre className="structure-output">{JSON.stringify({ nodes, edges }, null, 2)}</pre>
            </div>
        </div>
    );
}