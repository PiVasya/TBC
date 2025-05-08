// clientapp/src/hooks/useFlow.js
import { useRef, useCallback, useEffect } from 'react';
import {
    useNodesState,
    useEdgesState,
    addEdge,
    applyEdgeChanges,
    MarkerType
} from 'reactflow';

export function attachCallbacks(rawNodes, setNodes, setEdges) {
    return rawNodes.map(n => ({
        ...n,
        data: {
            ...n.data,
            onDelete: nid => {
                console.log('[NodeEditor] onDelete node', nid);
                setNodes(cur => cur.filter(x => x.id !== nid));
                setEdges(cur => cur.filter(e => e.source !== nid && e.target !== nid));
            },
            onEdit: nid => console.log('[NodeEditor] onEdit node', nid),
        }
    }));
}

export default function useFlow() {
    const idRef = useRef(1);
    const [nodes, setNodes, onNodesChange] = useNodesState([]);
    const [edges, setEdges, onEdgesChange] = useEdgesState([]);

    useEffect(() => {
        const maxId = nodes.reduce((mx, n) => {
            const num = Number(n.id);
            return isNaN(num) ? mx : Math.max(mx, num);
        }, 0);
        idRef.current = maxId + 1
    }, [nodes]);

    const handleEdgesChange = useCallback(changes => {
        const filtered = changes.filter(c => {
            if (c.type === 'remove') {
                const e = edges.find(x => x.id === c.id);
                return !e?.data?.magnet;
            }
            return true;
        });
        setEdges(es => applyEdgeChanges(filtered, es));
    }, [edges, setEdges]);

    const onConnect = useCallback(params => {
        setEdges(es =>
            addEdge(
                { ...params, markerEnd: { type: MarkerType.ArrowClosed }, data: { magnet: false } },
                es
            )
        );
    }, [setEdges]);

    const onEdgeDoubleClick = useCallback((_, edge) => {
        if (!edge.data?.magnet) {
            setEdges(es => es.filter(e => e.id !== edge.id));
        }
    }, [setEdges]);

    const onNodeDragStop = useCallback((_, dragged) => {
        // ——— ваш «магнитный» код из прошлого хука ———
        if (dragged.type === 'ButtonNode') {
            // вернуть на место уже приклеенные
            const mag = edges.find(e => e.data?.magnet && e.target === dragged.id);
            if (mag) {
                const src = nodes.find(n => n.id === mag.source);
                if (src) {
                    setNodes(nds =>
                        nds.map(n =>
                            n.id === dragged.id
                                ? { ...n, position: { x: src.position.x, y: src.position.y + (src.height || dragged.height || 40) } }
                                : n
                        )
                    );
                }
                return;
            }
            // попытаться приклеиться заново
            const SNAP = 30, w = dragged.width || 120, h = dragged.height || 40;
            nodes.forEach(target => {
                if (target.id === dragged.id) return;
                if (!['ButtonNode', 'ActionNode'].includes(target.type)) return;
                const tx = target.position.x + (target.width || w) / 2;
                const ty = target.position.y + (target.height || h);
                const dx = dragged.position.x + w / 2, dy = dragged.position.y;
                if (Math.abs(tx - dx) < SNAP && Math.abs(ty - dy) < SNAP) {
                    setNodes(nds =>
                        nds.map(n =>
                            n.id === dragged.id
                                ? { ...n, position: { x: target.position.x, y: target.position.y + (target.height || h) } }
                                : n
                        )
                    );
                    setEdges(es =>
                        addEdge(
                            {
                                id: `mag-${target.id}-${dragged.id}`, source: target.id, target: dragged.id,
                                markerEnd: { type: MarkerType.ArrowClosed }, data: { magnet: true }
                            },
                            es
                        )
                    );
                }
            });
        }
        if (dragged.type === 'ActionNode') {
            // перенести всех детей
            const adj = {};
            edges.forEach(e => {
                if (e.data?.magnet) {
                    (adj[e.source] = adj[e.source] || []).push(e.target);
                }
            });
            const newPos = { [dragged.id]: { x: dragged.position.x, y: dragged.position.y } };
            const compute = pid => {
                (adj[pid] || []).forEach(cid => {
                    const parent = newPos[pid] || nodes.find(n => n.id === pid).position;
                    const ph = nodes.find(n => n.id === pid)?.height || 40;
                    newPos[cid] = { x: parent.x, y: parent.y + ph };
                    compute(cid);
                });
            };
            compute(dragged.id);
            setNodes(nds => nds.map(n => newPos[n.id] ? { ...n, position: newPos[n.id] } : n));
        }
    }, [nodes, edges, setNodes, setEdges]);

    const addNode = useCallback(type => {
        const id = String(idRef.current++);
        const offset = 150;
        setNodes(nds => [
            ...nds,
            {
                id,
                type,
                data: {
                    label: `${type} ${id}`,
                    onDelete: nid => {
                        console.log('🔥 delete called for node', nid);
                        setNodes(n => n.filter(x => x.id !== nid));
                        setEdges(e => e.filter(x => x.source !== nid && x.target !== nid));
                    },
                    onEdit: nid => console.log('edit', nid)
                },
                position: { x: (nds.length % 3) * offset + 50, y: Math.floor(nds.length / 3) * offset + 50 }
            }
        ]);
    }, [setNodes, setEdges]);

    return {
        nodes, edges,
        setNodes, setEdges,
        onNodesChange,
        onEdgesChange: handleEdgesChange,
        onConnect,
        onEdgeDoubleClick,
        onNodeDragStop,
        addNode
    };
}
