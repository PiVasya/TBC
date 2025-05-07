// clientapp/src/App.jsx
import React, { useState, useEffect } from 'react';
import { listBots, getBot, createBot, updateBot, deleteBot } from './api/botApi';
import NodeEditor from './components/NodeEditor';
import './App.css';

export default function App() {
    const [mode, setMode] = useState('list');  // 'list' | 'new' | 'edit'
    const [bots, setBots] = useState([]);
    const [selected, setSelected] = useState(null);

    useEffect(() => {
        fetchBots();
    }, []);

    async function fetchBots() {
        try {
            const list = await listBots();
            setBots(list);
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
            fetchBots();
        } catch (e) {
            alert('Ошибка удаления: ' + e.message);
        }
    }

    function onCreated(dto) {
        setSelected(dto);
        setMode('edit');
        fetchBots();
    }

    return (
        <div className="app-container">
            {mode === 'list' && (
                <div className="panel">
                    <h2>Список ботов</h2>
                    <button className="app-button sm" onClick={startNew}>+ Новый бот</button>
                    <ul style={{ paddingLeft: 0, listStyle: 'none' }}>
                        {bots.map(b => (
                            <li key={b.id} style={{ margin: '8px 0' }}>
                                <strong>{b.name}</strong> [{b.status}]
                                <div className="button-group">
                                    <button className="app-button sm" onClick={() => openEdit(b)}>Открыть</button>
                                    <button className="app-button outline sm" onClick={() => remove(b)}>Удалить</button>
                                </div>
                            </li>
                        ))}
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
                        onCreated={onCreated}
                    />
                </div>
            )}
        </div>
    );
}
