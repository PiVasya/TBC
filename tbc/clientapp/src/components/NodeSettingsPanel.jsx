//src/components/NodeSettingsPanel.jsx
import React, { useState, useEffect } from 'react';
//import './NodeSettingsPanel.css';

export default function NodeSettingsPanel({ node, onClose, onSave }) {
    const [form, setForm] = useState({ ...node.data });

    useEffect(() => {
        setForm({ ...node.data });
    }, [node]);

    const handleChange = e => {
        const { name, value, type, checked } = e.target;
        setForm(f => ({
            ...f,
            [name]: type === 'checkbox' ? checked : value
        }));
    };

    const handleSubmit = e => {
        e.preventDefault();
        onSave(form);
        onClose();
    };

    return (
        <div className="node-settings-backdrop">
            <div
                className="node-settings-panel"
                style={{
                    position: 'absolute',
                    bottom: 0,
                    left: 0,
                    right: 0,
                    maxHeight: '40%',
                    overflowY: 'auto'
                }}
            >
                <h3>Настройки {node.type}</h3>
                <form onSubmit={handleSubmit}>
                    {node.type === 'TextNode' && (
                        <>
                            <label>Текст ноды</label>
                            <textarea
                                name="label"
                                value={form.label || ''}
                                onChange={handleChange}
                                rows={4}
                            />
                        </>
                    )}

                    {node.type === 'ActionNode' && (
                        <>
                            <label>
                                <input
                                    type="checkbox"
                                    name="saveToDb"
                                    checked={form.saveToDb || false}
                                    onChange={handleChange}
                                /> Сохранять ответ в БД
                            </label>
                            <label>
                                <input
                                    type="checkbox"
                                    name="notifyAdmin"
                                    checked={form.notifyAdmin || false}
                                    onChange={handleChange}
                                /> Уведомить администратора
                            </label>
                        </>
                    )}

                    {node.type === 'ButtonNode' && (
                        <>
                            <label>Текст на кнопке</label>
                            <input
                                name="label"
                                value={form.label || ''}
                                onChange={handleChange}
                            />
                        </>
                    )}

                    {node.type === 'StartNode' && (
                        <>
                            <label>Команда запуска</label>
                            <input
                                name="command"
                                value={form.command || ''}
                                onChange={handleChange}
                            />
                        </>
                    )}

                    <div className="actions">
                        <button type="button" className="app-button outline" onClick={onClose}>
                            Отмена
                        </button>
                        <button type="submit" className="app-button">
                            Сохранить
                        </button>
                    </div>
                </form>
            </div>
        </div>
    );
}
