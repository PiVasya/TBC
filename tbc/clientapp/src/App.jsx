import React, { useState, useEffect } from 'react';
import { getContainers, startContainer, stopContainer, deleteContainer } from './api/containerApi';
import DockerMenu from './components/DockerMenu';
import './App.css';
import NodeEditor from './components/NodeEditor';

function App() {
    const [showEditor, setShowEditor] = useState(false);

    const [token, setToken] = useState('');
    const [code, setCode] = useState('');
    const [proj, setProj] = useState('');
    const [docker, setDocker] = useState('');
    const [containerId, setContainerId] = useState(null);
    const [creating, setCreating] = useState(false);

    const [containers, setContainers] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');

    const [search, setSearch] = useState('');
    const [onlyBots, setOnlyBots] = useState(false);

    const fetchContainers = async () => {
        setLoading(true);
        setError('');
        try {
            setContainers(await getContainers());
        } catch (e) {
            setError(e.message);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchContainers();
    }, []);

    const displayed = containers.filter(c => {
        if (search && !c.name.includes(search)) return false;
        if (onlyBots && !c.name.startsWith('bot_')) return false;
        return true;
    });

    const onSubmit = async e => {
        e.preventDefault();
        setCreating(true);
        const form = new FormData();
        form.append('BotToken', token);
        form.append('BotCode', code);
        form.append('BotProj', proj);
        form.append('BotDocker', docker);

        try {
            const res = await fetch('/api/bots/create', { method: 'POST', body: form });
            const json = await res.json();
            if (!res.ok) throw new Error(json.error || 'Неизвестная ошибка');
            setContainerId(json.containerId);
            await fetchContainers();
        } catch (err) {
            alert('Ошибка: ' + err.message);
        } finally {
            setCreating(false);
        }
    };

    const toggleContainer = async c => {
        try {
            if (c.status === 'running') await stopContainer(c.id);
            else await startContainer(c.id);
            await fetchContainers();
        } catch (e) {
            alert(e.message);
        }
    };

    const onDelete = async c => {
        if (!window.confirm(`Удалить контейнер ${c.name}?`)) return;
        try {
            await deleteContainer(c.id);
            await fetchContainers();
        } catch (e) {
            alert(e.message);
        }
    };

    return (
        <div className="app-container">
            <button
                className="toggle-editor-button"
                onClick={() => setShowEditor(prev => !prev)}
            >
                {showEditor ? '← Вернуться к контейнерам' : 'Открыть редактор нод'}
            </button>

            {showEditor ? (
                <div className="editor-view">
                    <NodeEditor />
                </div>
            ) : (
                <>
                    <h1>Запустить Telegram-бота</h1>

                    <form onSubmit={onSubmit}>
                        <input
                            className="form-input"
                            value={token}
                            onChange={e => setToken(e.target.value)}
                            placeholder="Токен бота"
                            required
                        />
                        <textarea
                            className="form-textarea"
                            value={code}
                            onChange={e => setCode(e.target.value)}
                            placeholder="Код бота (необязательно)"
                        />
                        <input
                            className="form-input"
                            value={proj}
                            onChange={e => setProj(e.target.value)}
                            placeholder=".csproj (необязательно)"
                        />
                        <input
                            className="form-input"
                            value={docker}
                            onChange={e => setDocker(e.target.value)}
                            placeholder="Dockerfile (необязательно)"
                        />
                        <button className="form-button" type="submit" disabled={creating}>
                            {creating ? 'Запускаем...' : 'Запустить бота'}
                        </button>
                    </form>

                    {containerId && (
                        <p className="result">
                            Контейнер запущен: <code>{containerId}</code>
                        </p>
                    )}

                    <h2>Список контейнеров</h2>
                    {loading ? (
                        <p>Загрузка...</p>
                    ) : error ? (
                        <p className="error">Ошибка: {error}</p>
                    ) : (
                        <>
                            <div className="filters">
                                <input
                                    type="text"
                                    placeholder="Поиск по имени…"
                                    value={search}
                                    onChange={e => setSearch(e.target.value)}
                                />
                                <label>
                                    <input
                                        type="checkbox"
                                        checked={onlyBots}
                                        onChange={e => setOnlyBots(e.target.checked)}
                                    />
                                    Только боты
                                </label>
                            </div>
                            <DockerMenu
                                items={displayed}
                                onToggle={toggleContainer}
                                onDelete={onDelete}
                            />
                        </>
                    )}
                </>
            )}
        </div>
    );
}

export default App;
