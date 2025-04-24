import { useState } from 'react';
import DockerMenu from './components/DockerMenu';
import './App.css';

function App() {
    const [token, setToken] = useState('');
    const [code, setCode] = useState('');
    const [proj, setProj] = useState('');
    const [docker, setDocker] = useState('');
    const [containerId, setContainerId] = useState(null);
    const [creating, setCreating] = useState(false);

    const onSubmit = async (e) => {
        e.preventDefault();
        setCreating(true);
        const form = new FormData();
        form.append('BotToken', token);
        form.append('BotCode', code);
        form.append('BotProj', proj);
        form.append('BotDocker', docker);

        try {
            const res = await fetch('/api/bots/create', {
                method: 'POST',
                body: form
            });
            const json = await res.json();
            if (!res.ok) throw new Error(json.error || 'Неизвестная ошибка');
            setContainerId(json.containerId);
        } catch (err) {
            alert('Ошибка: ' + err.message);
        } finally {
            setCreating(false);
        }
    };

    // пока просто заглушка для меню — позже будем подтягивать из API
    const dummyContainers = [
        { id: '1a2b', name: 'bot-1234' },
        { id: '2b3c', name: 'bot-5678' },
        { id: '3c4d', name: 'bot-9012' },
        // ... можно добавить ещё, чтобы была прокрутка
    ];

    return (
        <div className="app-container">
            <h1>Запустить Telegram-бота</h1>
            <form onSubmit={onSubmit}>
                <input
                    className="form-input"
                    type="text"
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
                    type="text"
                    value={proj}
                    onChange={e => setProj(e.target.value)}
                    placeholder=".csproj (необязательно)"
                />
                <input
                    className="form-input"
                    type="text"
                    value={docker}
                    onChange={e => setDocker(e.target.value)}
                    placeholder="Dockerfile (необязательно)"
                />
                <button
                    className="form-button"
                    type="submit"
                    disabled={creating}
                >
                    {creating ? 'Запускаем...' : 'Запустить бота'}
                </button>
            </form>

            {containerId && (
                <p className="result">
                    Контейнер запущен: <code>{containerId}</code>
                </p>
            )}

            <DockerMenu items={dummyContainers} />
        </div>
    );
}

export default App;
