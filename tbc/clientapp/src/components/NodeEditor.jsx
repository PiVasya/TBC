// clientapp/src/components/NodeEditor.jsx
import React, { useState, useEffect, useCallback } from 'react';
import ReactFlow, {
    ReactFlowProvider,
    Background,
    Controls
} from 'reactflow';
import 'reactflow/dist/style.css';
import './NodeEditor.css';

import useFlow, { attachCallbacks } from '../hooks/useFlow';
import { listSchemas, getSchema, postSchema } from '../api/schemaApi';
import { createBot, updateBot } from '../api/botApi';

// Динамический импорт типов нод один раз
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
    onCreated
}) {
    const [name, setName] = useState(initialName);
    const [token, setToken] = useState(initialToken);
    const [versions, setVersions] = useState([]);
    const [selectedVersion, setSelectedVersion] = useState(null);
    const [dirty, setDirty] = useState(false);

    // Хук для flow-логики
    const {
        nodes,
        edges,
        onNodesChange: rawOnNodesChange,
        onEdgesChange: rawOnEdgesChange,
        onConnect: rawOnConnect,
        onEdgeDoubleClick: rawOnEdgeDoubleClick,
        onNodeDragStop: rawOnNodeDragStop,
        addNode,
        setNodes,
        setEdges
    } = useFlow();

    // Обёртки с логированием
    const onNodesChange = useCallback(changes => {
        console.log('[NodeEditor] onNodesChange:', changes);
        rawOnNodesChange(changes);
    }, [rawOnNodesChange]);

    const onEdgesChange = useCallback(changes => {
        console.log('[NodeEditor] onEdgesChange:', changes);
        rawOnEdgesChange(changes);
    }, [rawOnEdgesChange]);

    const onConnect = useCallback(params => {
        console.log('[NodeEditor] onConnect:', params);
        rawOnConnect(params);
    }, [rawOnConnect]);

    const onEdgeDoubleClick = useCallback((event, edge) => {
        console.log('[NodeEditor] onEdgeDoubleClick:', edge);
        rawOnEdgeDoubleClick(event, edge);
    }, [rawOnEdgeDoubleClick]);

    const onNodeDragStop = useCallback((event, node) => {
        console.log('[NodeEditor] onNodeDragStop:', node);
        rawOnNodeDragStop(event, node);
    }, [rawOnNodeDragStop]);

    // «Грязность» формы
    useEffect(() => {
        const schemaChanged =
            JSON.stringify({ nodes, edges }) !== JSON.stringify({ nodes: [], edges: [] });
        setDirty(
            name !== initialName ||
            token !== initialToken ||
            schemaChanged
        );
    }, [name, token, nodes, edges, initialName, initialToken]);

    // Загрузка версий схемы
    useEffect(() => {
        if (!botId) return;
        (async () => {
            console.log('[NodeEditor] fetching schema versions for bot', botId);
            const vs = await listSchemas(botId);
            setVersions(vs);
            if (!vs.length) return;

            const latest = vs[0];
            setSelectedVersion(latest.id);
            console.log('[NodeEditor] loading latest schema id', latest.id);

            const schema = await getSchema(botId, latest.id);
            setNodes(
                (schema.nodes || []).map(n => ({
                    ...n,
                    data: {
                        ...n.data,
                        onDelete: nid => {
                            console.log('[NodeEditor] onDelete node', nid);
                            setNodes(cur => cur.filter(x => x.id !== nid));
                            setEdges(cur => cur.filter(e => e.source !== nid && e.target !== nid));
                        },
                        onEdit: nid => console.log('[NodeEditor] onEdit node', nid)
                    }
                }))
            );
            setEdges(schema.edges || []);
        })();
    }, [botId, setNodes, setEdges]);

    // Сохранение
    const handleSave = async () => {
        if (!dirty) return;
        console.log('[NodeEditor] saving, dirty=', dirty);
        if (!botId) {
            const dto = await createBot({ name, telegramToken: token, schema: { nodes, edges } });
            console.log('[NodeEditor] created bot', dto);
            onCreated(dto);
        } else {
            await updateBot(botId, { name, telegramToken: token });
            await postSchema(botId, { nodes, edges });
            alert('Изменения сохранены');
            console.log('[NodeEditor] updated bot & posted schema');
        }
        setDirty(false);
    };

    return (
        <div className="node-editor-container">
            <div className="sidebar">
                <button className="app-button outline sm back-btn" onClick={onBack}>
                    ← Вернуться
                </button>

                <h3>{botId ? 'Редактирование бота' : 'Новый бот'}</h3>

                <label>
                    Имя бота<br />
                    <input
                        className="form-input"
                        value={name}
                        onChange={e => setName(e.target.value)}
                    />
                </label>

                <label>
                    Telegram-токен<br />
                    <input
                        className="form-input"
                        value={token}
                        onChange={e => setToken(e.target.value)}
                    />
                </label>

                <button
                    className="app-button sm"
                    disabled={!dirty}
                    onClick={handleSave}
                >
                    💾 Сохранить
                </button>

                <h3>Версии схемы</h3>
                <div style={{ margin: '0.5rem 0' }}>
                    <select
                        className="form-input"
                        value={selectedVersion || ''}
                        onChange={async e => {
                            const vid = Number(e.target.value);
                            console.log('[NodeEditor] switching to version', vid);
                            setSelectedVersion(vid);
                            const schema = await getSchema(botId, vid);
                            setNodes(attachCallbacks(schema.nodes || []));
                            setEdges(schema.edges || []);
                        }}
                    >
                        <option value="" disabled>Выберите версию…</option>
                        {versions.map(v => (
                            <option key={v.id} value={v.id}>
                                {v.id} — {new Date(v.createdAt).toLocaleString()}
                            </option>
                        ))}
                    </select>
                    <button
                        className="app-button sm"
                        style={{ marginLeft: 8 }}
                        onClick={async () => {
                            if (!selectedVersion) return;
                            console.log('[NodeEditor] reloading version', selectedVersion);
                            const schema = await getSchema(botId, selectedVersion);
                            setNodes(attachCallbacks(schema.nodes || []));
                            setEdges(schema.edges || []);
                        }}
                    >
                        🔄 Загрузить
                    </button>
                </div>

                <h3>Типы нод</h3>
                {Object.keys(nodeTypes).map(t => (
                    <button
                        key={t}
                        className="app-button sm"
                        onClick={() => {
                            console.log('[NodeEditor] addNode', t);
                            addNode(t);
                        }}
                    >
                        {t}
                    </button>
                ))}
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
        </div>
    );
}
