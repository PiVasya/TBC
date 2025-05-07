// clientapp/src/components/NodeEditor.jsx
import React, { useState, useEffect } from 'react';
import ReactFlow, { ReactFlowProvider, Background, Controls } from 'reactflow';
import 'reactflow/dist/style.css';
import './NodeEditor.css';
import useFlow from '../hooks/useFlow';
import { listSchemas, getSchema, postSchema } from '../api/schemaApi';
import { createBot, updateBot } from '../api/botApi';

export default function NodeEditor({
    botId,               // если есть — редактируем, иначе — создаём
    initialName,
    initialToken,
    onBack,
    onCreated           // callback(data: BotDto) при создании
}) {
    const [name, setName] = useState(initialName);
    const [token, setToken] = useState(initialToken);
    const [versions, setVersions] = useState([]);
    const [selectedVersion, setSelectedVersion] = useState(null);

    const flow = useFlow();
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
    } = flow;

    const [dirty, setDirty] = useState(false);

    // определяем «грязность»: любое изменение полей или схемы
    useEffect(() => {
        const schemaChanged =
            JSON.stringify({ nodes, edges }) !== JSON.stringify({ nodes: [], edges: [] });
        setDirty(
            name !== initialName ||
            token !== initialToken ||
            schemaChanged
        );
    }, [name, token, nodes, edges, initialName, initialToken]);

    useEffect(() => {
        if (!botId) return;
        (async () => {
            // 1) получить список версий
            const vs = await listSchemas(botId);
            setVersions(vs);
            if (vs.length === 0) return;

            // 2) сразу выбрать самую первую (самую свежую) версию
            const latest = vs[0];
            setSelectedVersion(latest.id);

            // 3) и загрузить по ней схему
            const schema = await getSchema(botId, latest.id);
            setNodes(schema.nodes || []);
            setEdges(schema.edges || []);

        })();
    }, [botId, setNodes, setEdges]);

    const handleSave = async () => {
        if (!dirty) return;

        if (!botId) {
            // создание нового бота вместе со схемой
            const dto = await createBot({
                name,
                telegramToken: token,
                schema: { nodes, edges }
            });
            onCreated(dto);
        } else {
            // обновить имя и токен существующего бота
            await updateBot(botId, { name, telegramToken: token });
            // сохранить новую версию схемы
            await postSchema(botId, { nodes, edges });
            alert('Изменения сохранены');
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

                {/* ——————————————————————————————————— */}
                <h3>Версии схемы</h3>
                <div style={{ margin: '0.5rem 0' }}>
                    <select
                        className="form-input"
                        value={selectedVersion || ''}
                        onChange={async e => {
                            const vid = Number(e.target.value);
                            setSelectedVersion(vid);
                            const schema = await getSchema(botId, vid);
                            setNodes(schema.nodes || []);
                            setEdges(schema.edges || []);
                        }}
                    >
                        <option value="" disabled>
                            Выберите версию…
                        </option>
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
                            const schema = await getSchema(botId, selectedVersion);
                            setNodes(schema.nodes || []);
                            setEdges(schema.edges || []);
                        }}
                    >
                        🔄 Загрузить
                    </button>
                </div>
                {/* ——————————————————————————————————— */}

                <h3>Типы нод</h3>
                {['StartNode', 'TextNode', 'ActionNode', 'ButtonNode'].map(t => (
                    <button
                        key={t}
                        className="app-button sm"
                        onClick={() => addNode(t)}
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
                        nodeTypes={require.context('./nodes', false, /\.jsx$/)
                            .keys()
                            .reduce((acc, path) => {
                                const name = path.replace('./', '').replace('.jsx', '');
                                acc[name] = require('./nodes/' + path.replace('./', '')).default;
                                return acc;
                            }, {})}
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
