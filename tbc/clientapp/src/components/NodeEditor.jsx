import React, { useState, useEffect, useCallback } from 'react';
import ReactFlow, { ReactFlowProvider, Background, Controls } from 'reactflow';
import 'reactflow/dist/style.css';
import './NodeEditor.css';
import useFlow, { attachCallbacks } from '../hooks/useFlow';
import { listSchemas, getSchema, postSchema } from '../api/schemaApi';
import { createBot, rebuildBot } from '../api/botApi';
import NodeSettingsPanel from './InlineNodeEditor';

const req = require.context('./nodes', false, /\.jsx$/);
const nodeTypes = req.keys().reduce((acc, path) => {
    const mod = req(path);
    const name = path.replace('./', '').replace('.jsx', '');
    acc[name] = mod.default;
    return acc;
}, {});

export default function NodeEditor({
    botId,
    initialName,
    initialToken,
    onBack,
    onCreated,
    onRebuilt
}) {
    const [name, setName] = useState(initialName);
    const [token, setToken] = useState(initialToken);
    const [versions, setVersions] = useState([]);
    const [selectedVersion, setSelectedVersion] = useState(null);
    const [dirty, setDirty] = useState(false);
    const [editingNode, setEditingNode] = useState(null);

    const {
        nodes,
        edges,
        onNodesChange,
        onEdgesChange,
        onConnect,
        onEdgeDoubleClick,
        onNodeDragStop,
        addNode,
        setNodes,
        setEdges
    } = useFlow();

    const enrichNode = useCallback(n => ({
        ...n,
        data: {
            ...n.data,
            onDelete: id => {
                setNodes(xs => xs.filter(x => x.id !== id));
                setEdges(es => es.filter(e => e.source !== id && e.target !== id));
            },
            onEdit: () => setEditingNode(n)
        }
    }), [setNodes, setEdges]);

    useEffect(() => {
        const changed =
            JSON.stringify({ nodes, edges }) !== JSON.stringify({ nodes: [], edges: [] });
        setDirty(
            name !== initialName ||
            token !== initialToken ||
            changed
        );
    }, [name, token, nodes, edges, initialName, initialToken]);

    useEffect(() => {
        if (!botId) return;
        (async () => {
            const vs = await listSchemas(botId);
            setVersions(vs);
            if (!vs.length) return;

            const latest = vs[0];
            setSelectedVersion(latest.id);

            const schema = await getSchema(botId, latest.id);
            setNodes(attachCallbacks(schema.nodes || [], setNodes, setEdges));
            setEdges(schema.edges);
        })();
    }, [botId, enrichNode, setNodes, setEdges]);

    const handleSave = async () => {
        if (!dirty) return;
        if (!botId) {
            const dto = await createBot({ name, telegramToken: token, schema: { nodes, edges } });
            onCreated(dto);
        } else {
            await postSchema(botId, { nodes, edges });
            await rebuildBot(botId, { name, telegramToken: token });
            onRebuilt();
        }
        setDirty(false);
    };

    return (
        <div className="node-editor-container">
            <div className="sidebar">
                <button className="app-button outline sm" onClick={onBack}>← Назад</button>
                <h3>{botId ? 'Редактировать бота' : 'Новый бот'}</h3>

                <label>Имя бота<br />
                    <input className="form-input" value={name} onChange={e => setName(e.target.value)} />
                </label>
                <label>Telegram-токен<br />
                    <input className="form-input" value={token} onChange={e => setToken(e.target.value)} />
                </label>

                <button className="app-button sm" disabled={!dirty} onClick={handleSave}>💾 Сохранить</button>

                <h3>Версии схемы</h3>
                <div style={{ margin: '0.5rem 0' }}>
                    <select
                        className="form-input"
                        value={selectedVersion || ''}
                        onChange={async e => {
                            const vid = +e.target.value;
                            setSelectedVersion(vid);
                            const schema = await getSchema(botId, vid);
                            setNodes(attachCallbacks(schema.nodes || [], setNodes, setEdges));
                            setEdges(schema.edges);
                        }}
                    >
                        <option value="" disabled>Выберите…</option>
                        {versions.map(v =>
                            <option key={v.id} value={v.id}>
                                {v.id} — {new Date(v.createdAt).toLocaleString()}
                            </option>
                        )}
                    </select>
                    <button
                        className="app-button sm"
                        style={{ marginLeft: 8 }}
                        onClick={async () => {
                            if (!selectedVersion) return;
                            const schema = await getSchema(botId, selectedVersion);
                            setNodes(attachCallbacks(schema.nodes || [], setNodes, setEdges));
                            setEdges(schema.edges);
                        }}
                    >🔄 Загрузить</button>
                </div>

                <h3>Типы нод</h3>
                {Object.keys(nodeTypes).map(t =>
                    <button
                        key={t}
                        className="app-button sm"
                        onClick={() => addNode(t)}
                    >{t}</button>
                )}
            </div>

            <div className="canvas-area">
                <ReactFlowProvider>
                    <ReactFlow
                        nodes={nodes}
                        edges={edges}
                        onNodesChange={onNodesChange}
                        onEdgesChange={onEdgesChange}
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
            </div>

            {editingNode && (
                <NodeSettingsPanel
                    node={editingNode}
                    onClose={() => setEditingNode(null)}
                    onSave={upd => {
                        setNodes(ns => ns.map(n =>
                            n.id === editingNode.id
                                ? { ...n, data: { ...n.data, ...upd } }
                                : n
                        ));
                    }}
                />
            )}
        </div>
    );
}
