// src/components/InlineNodeEditor.jsx
import React, { useState, useEffect } from 'react';
import './InlineNodeEditor.css';

export default function InlineNodeEditor({ data, onSave, onCancel, schemaFields }) {
    // schemaFields — массив объектов вида:
    // { name: string, label: string, type: 'text'|'textarea'|'checkbox'|'select', options?: { value: string, label: string }[], rows?: number }
    const [form, setForm] = useState({});

    useEffect(() => {
        // инициализируем форму текущими значениями из data или дефолтами
        const initial = {};
        schemaFields.forEach(fld => {
            if (fld.type === 'checkbox') {
                initial[fld.name] = Boolean(data[fld.name]);
            } else {
                initial[fld.name] = data[fld.name] ?? '';
            }
        });
        setForm(initial);
    }, [data, schemaFields]);

    const handleChange = e => {
        const { name, value, type, checked } = e.target;
        setForm(prev => ({
            ...prev,
            [name]: type === 'checkbox' ? checked : value
        }));
    };

    const handleSubmit = e => {
        e.preventDefault();
        onSave(form);
    };

    return (
        <div className="inline-editor">
            <form onSubmit={handleSubmit}>
                {schemaFields.map(fld => (
                    <div key={fld.name} className="field">
                        <label htmlFor={fld.name}>{fld.label}</label>

                        {fld.type === 'textarea' && (
                            <textarea
                                id={fld.name}
                                name={fld.name}
                                value={form[fld.name]}
                                onChange={handleChange}
                                rows={fld.rows || 3}
                            />
                        )}

                        {fld.type === 'checkbox' && (
                            <input
                                id={fld.name}
                                type="checkbox"
                                name={fld.name}
                                checked={form[fld.name]}
                                onChange={handleChange}
                            />
                        )}

                        {fld.type === 'select' && (
                            <select
                                id={fld.name}
                                name={fld.name}
                                value={form[fld.name]}
                                onChange={handleChange}
                            >
                                {fld.options.map(opt => (
                                    <option key={opt.value} value={opt.value}>
                                        {opt.label}
                                    </option>
                                ))}
                            </select>
                        )}

                        {fld.type === 'text' && (
                            <input
                                id={fld.name}
                                type="text"
                                name={fld.name}
                                value={form[fld.name]}
                                onChange={handleChange}
                            />
                        )}
                    </div>
                ))}

                <div className="actions">
                    <button
                        type="button"
                        className="app-button outline sm"
                        onClick={onCancel}
                    >
                        Отмена
                    </button>
                    <button type="submit" className="app-button sm">
                        Сохранить
                    </button>
                </div>
            </form>
        </div>
    );
}
