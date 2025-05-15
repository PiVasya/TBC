// clientapp/src/api/containerApi.js
const API_BASE = '/api/containers';

export async function getContainers() {
    const res = await fetch(API_BASE);
    if (!res.ok) throw new Error('Не удалось получить список контейнеров');
    return res.json();
}

export async function startContainer(id) {
    const res = await fetch(`${API_BASE}/${id}/start`, { method: 'POST' });
    if (!res.ok) throw new Error(`Не удалось запустить контейнер ${id}`);
}

export async function stopContainer(id) {
    const res = await fetch(`${API_BASE}/${id}/stop`, { method: 'POST' });
    if (!res.ok) throw new Error(`Не удалось остановить контейнер ${id}`);
}

export async function deleteContainer(id) {
    const res = await fetch(`/api/containers/${id}`, { method: 'DELETE' });
    if (!res.ok) throw new Error(`Не удалось удалить контейнер ${id}`);
}
