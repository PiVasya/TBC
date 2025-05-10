// clientapp/src/api/botApi.js
const BASE = '/api/bots';

/**
 * Получить список всех ботов.
 */
export async function listBots() {
    const res = await fetch(BASE);
    if (!res.ok) throw new Error(res.statusText);
    return res.json();
}

/**
 * Создать нового бота.
 * @param {{ name:string, telegramToken:string, schema:any, botCode?:string, botProj?:string, botDocker?:string }} data
 */
export async function createBot(data) {
    const res = await fetch(BASE, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(data),
    });
    if (!res.ok) {
        const err = await res.json();
        throw new Error(err.error || res.statusText);
    }
    return res.json();
}

/**
 * Удалить бота.
 */
export async function deleteBot(id) {
    const res = await fetch(`${BASE}/${id}`, { method: 'DELETE' });
    if (!res.ok) {
        const err = await res.json();
        throw new Error(err.error || res.statusText);
    }
}

/**
 * Обновить имя и токен существующего бота
 * @param {number} id
 * @param {{ name: string, telegramToken: string }} data
 */
export async function updateBot(id, data) {
    const res = await fetch(`${BASE}/${id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            name: data.name,
            telegramToken: data.telegramToken
        }),
    });
    if (!res.ok) {
        const err = await res.json();
        throw new Error(err.error || res.statusText);
    }
    return res.json(); // вернёт обновлённый BotDto
}

export async function getBot(id) {
    const res = await fetch(`${BASE}/${id}`);
    if (!res.ok) {
        const err = await res.json();
        throw new Error(err.error || res.statusText);
    }
    return res.json(); // вернёт BotDto с полем telegramToken
}

export async function startBot(id) {
    const res = await fetch(`${BASE}/${id}/start`, { method: 'POST' });
    if (!res.ok) {
        const err = await res.json().catch(() => ({}));
        throw new Error(err.error || res.statusText);
    }
    // если нет контента — просто выходим
    if (res.status === 204) return;
    return res.json();
}

/**
 * Остановить контейнер бота
 */
export async function stopBot(id) {
    const res = await fetch(`${BASE}/${id}/stop`, { method: 'POST' });
    if (!res.ok) {
        const err = await res.json().catch(() => ({}));
        throw new Error(err.error || res.statusText);
    }
    if (res.status === 204) return;
    return res.json();
}

/**
 * Пересобрать (rebuild) контейнер бота
 */
export async function rebuildBot(id, { name, telegramToken }) {
    const res = await fetch(`${BASE}/${id}/rebuild`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ name, telegramToken })
    });
    if (!res.ok) {
        const err = await res.json().catch(() => ({}));
        throw new Error(err.error || res.statusText);
    }
    return res.json();
}