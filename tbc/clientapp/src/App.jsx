// clientapp/src/App.jsx
import React, { useState, useEffect } from 'react';
import {
    listBots,
    getBot,
    deleteBot,
    startBot,
    stopBot
} from './api/botApi';
import NodeEditor from './components/NodeEditor';
import { getContainers } from './api/containerApi';
import './App.css';
import './BotList.css';
import './Loading.css';

export default function App() {
    const [mode, setMode] = useState('list');  // 'list' | 'new' | 'edit'
    const [bots, setBots] = useState([]);
    const [containers, setContainers] = useState([]);
    const [selected, setSelected] = useState(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        fetchData();
    }, []);

    async function fetchData() {
        setLoading(true);
        try {
            const [botList, contList] = await Promise.all([
                listBots(),
                getContainers()
            ]);
            setBots(botList);
            setContainers(contList);
        } catch (e) {
            alert('Ошибка загрузки: ' + e.message);
        } finally {
            setLoading(false);
        }
    }

    function startNew() {
        setSelected(null);
        setMode('new');
    }

    function openEdit(bot) {
        getBot(bot.id)
            .then(full => {
                setSelected(full);
                setMode('edit');
            })
            .catch(e => alert('Не удалось загрузить бота: ' + e.message));
    }

    async function remove(bot) {
        if (!window.confirm(`Удалить "${bot.name}"?`)) return;
        try {
            await deleteBot(bot.id);
            await fetchData();
        } catch (e) {
            alert('Ошибка удаления: ' + e.message);
        }
    }

    const handleStart = async bot => {
        try {
            await startBot(bot.id);
            await fetchData();
        } catch (e) {
            alert('Не удалось запустить: ' + e.message);
        }
    };

    const handleStop = async bot => {
        try {
            await stopBot(bot.id);
            await fetchData();
        } catch (e) {
            alert('Не удалось остановить: ' + e.message);
        }
    };

    // после успешного создания или rebuild-а — вернуться в list и обновить
    const onDone = async () => {
        await fetchData();
        setMode('list');
    };

    return (
        <div className="app-container">
            {mode === 'list' && (
                loading
                    ? <div className="panel loading-panel">Загрузка…</div>
                    : <div className="panel">
                    <h2>Список ботов</h2>
                    <button className="app-button sm" onClick={startNew}>
                        + Новый бот
                    </button>

                    <ul className="bot-list">
                        {bots.map(b => {
                            const ctr = containers.find(c => c.id === b.containerId);
                            let cls = 'bot-row missing';
                            if (ctr) cls = ctr.status === 'running' ? 'bot-row running' : 'bot-row stopped';

                            return (
                                <li key={b.id} className={cls}>
                                    <div className="bot-info">
                                        <strong>{b.name}</strong>
                                        <span className="bot-status">
                                            {ctr ? (ctr.status === 'running' ? 'Running' : 'Stopped') : 'Not built'}
                                        </span>
                                    </div>
                                    <div className="button-group">
                                        {!ctr && (
                                            <button className="app-button sm" onClick={() => handleStart(b)}>
                                                Build & Start
                                            </button>
                                        )}
                                        {ctr && ctr.status !== 'running' && (
                                            <button className="app-button sm" onClick={() => handleStart(b)}>
                                                Start
                                            </button>
                                        )}
                                        {ctr && ctr.status === 'running' && (
                                            <button className="app-button outline sm" onClick={() => handleStop(b)}>
                                                Stop
                                            </button>
                                        )}
                                        <button className="app-button sm" onClick={() => openEdit(b)}>
                                            Edit
                                        </button>
                                        <button className="app-button outline sm" onClick={() => remove(b)}>
                                            Delete
                                        </button>
                                    </div>
                                </li>
                            );
                        })}
                    </ul>
                        </div>
            )}

            {(mode === 'new' || (mode === 'edit' && selected)) && (
                <div style={{ flex: 1 }}>
                    <NodeEditor
                        botId={mode === 'edit' ? selected.id : null}
                        initialName={mode === 'edit' ? selected.name : ''}
                        initialToken={mode === 'edit' ? selected.telegramToken : ''}
                        onBack={() => setMode('list')}
                        onCreated={onDone}   // после создания нового бота
                        onRebuilt={onDone}   // после rebuild при редактировании
                    />
                </div>
            )}
        </div>
    );
}
