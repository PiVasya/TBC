// clientapp/src/App.jsx
import React, { useState, useEffect } from 'react';
import {
    listBots,
    getBot,
    createBot,
    updateBot,
    deleteBot
} from './api/botApi';
import { getContainers } from './api/containerApi';
import NodeEditor from './components/NodeEditor';
import './App.css';
import './BotList.css';

export default function App() {
    const [mode, setMode] = useState('list');  // 'list' | 'new' | 'edit'
    const [bots, setBots] = useState([]);
    const [containers, setContainers] = useState([]);
    const [selected, setSelected] = useState(null);

    useEffect(() => {
        fetchData();
    }, []);

    async function fetchData() {
        try {
            const [botList, contList] = await Promise.all([
                listBots(),
                getContainers()
            ]);
            setBots(botList);
            setContainers(contList);
        } catch (e) {
            alert('Ошибка загрузки: ' + e.message);
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
            .catch(e => {
                alert('Не удалось загрузить бота: ' + e.message);
            });
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

    // Этот колбек теперь обязательно должен попасть в NodeEditor
    function onCreated(dto) {
        setSelected(dto);
        setMode('edit');
        fetchData();
    }

    return (
        <div className="app-container">
            {mode === 'list' && (
                <div className="panel">
                    <h2>Список ботов</h2>
                    <button className="app-button sm" onClick={startNew}>
                        + Новый бот
                    </button>

                    <ul className="bot-list">
                        {bots.map(b => {
                            const ctr = containers.find(c => c.id === b.containerId);
                            let statusClass = 'bot-row missing';
                            if (ctr) {
                                statusClass = ctr.status === 'running'
                                    ? 'bot-row running'
                                    : 'bot-row stopped';
                            }
                            return (
                                <li key={b.id} className={statusClass}>
                                    <strong>{b.name}</strong>
                                    <div className="button-group">
                                        <button
                                            className="app-button sm"
                                            onClick={() => openEdit(b)}
                                        >
                                            Открыть
                                        </button>
                                        <button
                                            className="app-button outline sm"
                                            onClick={() => remove(b)}
                                        >
                                            Удалить
                                        </button>
                                    </div>
                                </li>
                            );
                        })}
                    </ul>
                </div>
            )}

            {/* Рендерим редактор, когда создаём новый или редактируем */}
            {(mode === 'new' || (mode === 'edit' && selected)) && (
                <div style={{ flex: 1 }}>
                    <NodeEditor
                        botId={mode === 'edit' ? selected.id : null}
                        initialName={mode === 'edit' ? selected.name : ''}
                        initialToken={mode === 'edit' ? selected.telegramToken : ''}
                        onBack={() => setMode('list')}
                        onCreated={onCreated}
                    />
                </div>
            )}
        </div>
    );
}
