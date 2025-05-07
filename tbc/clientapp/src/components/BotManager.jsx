// clientapp/src/components/BotManager.jsx
import React, { useState, useEffect } from 'react';
import NodeEditor from './NodeEditor';
import {
    listBots,
    createBot,
    deleteBot
} from '../api/botApi';
import {
    listSchemas,
    getSchema,
    postSchema
} from '../api/schemaApi';

export default function BotManager() {
    const [mode, setMode] = useState('list');      // 'list' | 'new' | 'edit'
    const [bots, setBots] = useState([]);
    const [selected, setSelected] = useState(null); // выбранный BotDto
    const [schema, setSchema] = useState({ nodes: [], edges: [] });
    const [name, setName] = useState('');
    const [token, setToken] = useState('');
    const [schemaVersions, setSchemaVersions] = useState([]);
    const [chosenVersion, setChosenVersion] = useState(null);

    useEffect(() => {
        refreshBots();
    }, []);

    async function refreshBots() {
        try {
            const list = await listBots();
            setBots(list);
        } catch (e) {
            alert('Ошибка при загрузке ботов: ' + e.message);
        }
    }

    // ========== Режим «Создать нового» ==========
    const handleNew = () => {
        setName(''); setToken('');
        setSchema({ nodes: [], edges: [] });
        setMode('new');
    };

    const submitNew = async () => {
        try {
            const dto = await createBot({
                name,
                telegramToken: token,
                schema,
            });
            alert(`Бот создан (id=${dto.id})`);
            await refreshBots();
            setMode('list');
        } catch (e) {
            alert('Ошибка создания: ' + e.message);
        }
    };

    // ========== Режим «Редактировать» существующего ==========
    const openBot = async bot => {
        setSelected(bot);
        // Загрузить список версий схем
        const versions = await listSchemas(bot.id);
        setSchemaVersions(versions);
        setChosenVersion(versions[0]?.id ?? null);

        // Если есть хотя бы одна версия — загрузить её
        if (versions.length > 0) {
            const loaded = await getSchema(bot.id, versions[0].id);
            setSchema(loaded);
        } else {
            setSchema({ nodes: [], edges: [] });
        }
        setMode('edit');
    };

    const saveEdit = async () => {
        if (!selected) return;
        try {
            const res = await postSchema(selected.id, schema);
            alert(`Схема сохранена (версия id=${res.id})`);
            const versions = await listSchemas(selected.id);
            setSchemaVersions(versions);
            setChosenVersion(res.id);
        } catch (e) {
            alert('Ошибка сохранения схемы: ' + e.message);
        }
    };

    const deleteSelectedBot = async bot => {
        if (!window.confirm(`Удалить бота "${bot.name}"?`)) return;
        try {
            await deleteBot(bot.id);
            await refreshBots();
            setMode('list');
        } catch (e) {
            alert('Ошибка удаления: ' + e.message);
        }
    };

    // ========== Рендер ==========
    return (
        <div style={{ display: 'flex', height: '100%' }}>
            <div style={{ width: 240, padding: 16, borderRight: '1px solid #ccc' }}>
                {mode === 'list' && (
                    <>
                        <h2>Боты</h2>
                        <button onClick={handleNew}>+ Новый бот</button>
                        <ul>
                            {bots.map(b => (
                                <li key={b.id} style={{ margin: '8px 0' }}>
                                    <strong>{b.name}</strong> [{b.status}]
                                    <div style={{ marginTop: 4 }}>
                                        <button onClick={() => openBot(b)}>Открыть</button>{' '}
                                        <button onClick={() => deleteSelectedBot(b)}>Удалить</button>
                                    </div>
                                </li>
                            ))}
                        </ul>
                    </>
                )}

                {mode === 'new' && (
                    <>
                        <h2>Новый бот</h2>
                        <label>
                            Имя<br />
                            <input value={name} onChange={e => setName(e.target.value)} />
                        </label>
                        <br /><br />
                        <label>
                            Telegram-токен<br />
                            <input value={token} onChange={e => setToken(e.target.value)} />
                        </label>
                        <br /><br />
                        <button onClick={() => setMode('list')}>← Отмена</button>{' '}
                        <button onClick={submitNew}>Создать</button>
                    </>
                )}

                {mode === 'edit' && selected && (
                    <>
                        <h2>Бот: {selected.name}</h2>
                        <button onClick={() => setMode('list')}>← Назад</button>
                        <button style={{ marginLeft: 8 }} onClick={saveEdit}>
                            💾 Сохранить схему
                        </button>
                        <br /><br />
                        <label>
                            Выбрать версию схемы:<br />
                            <select
                                value={chosenVersion || ''}
                                onChange={async e => {
                                    const vid = Number(e.target.value);
                                    setChosenVersion(vid);
                                    const loaded = await getSchema(selected.id, vid);
                                    setSchema(loaded);
                                }}
                            >
                                {schemaVersions.map(v => (
                                    <option key={v.id} value={v.id}>
                                        {v.id} – {new Date(v.createdAt).toLocaleString()}
                                    </option>
                                ))}
                            </select>
                        </label>
                    </>
                )}
            </div>

            <div style={{ flex: 1, position: 'relative' }}>
                {(mode === 'new' || mode === 'edit') && (
                    <NodeEditor
                        initialSchema={schema}
                        onSchemaChange={setSchema}
                    />
                )}
            </div>
        </div>
    );
}
