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