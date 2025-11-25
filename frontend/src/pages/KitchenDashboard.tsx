import { useEffect, useState } from 'react'
import { KitchenApi } from '../services/api'
import { KitchenTaskDto } from '../types'

export default function KitchenDashboard() {
    const [tasks, setTasks] = useState<KitchenTaskDto[]>([])

    const loadTasks = async () => {
        try {
            const data = await KitchenApi.getAll()
            // Sortăm: cele mai vechi primele (FIFO)
            setTasks(data.sort((a, b) => new Date(a.updatedAt).getTime() - new Date(b.updatedAt).getTime()))
        } catch (err) {
            console.error("Failed to load tasks", err)
        }
    }

    useEffect(() => {
        loadTasks()
        const interval = setInterval(loadTasks, 5000) // Refresh la fiecare 5 secunde
        return () => clearInterval(interval)
    }, [])

    const advanceStatus = async (task: KitchenTaskDto) => {
        const statusMap: Record<string, string> = {
            'NotStarted': 'Preparing',
            'Preparing': 'Ready',
            'Ready': 'Completed'
        }
        
        const nextStatus = statusMap[task.status]
        if (!nextStatus) return

        try {
            await KitchenApi.updateStatus(task.id, nextStatus)
            loadTasks()
        } catch (err) {
            alert('Eroare la actualizarea statusului.')
        }
    }

    const columns = {
        'NotStarted': tasks.filter(t => t.status === 'NotStarted'),
        'Preparing': tasks.filter(t => t.status === 'Preparing'),
        'Ready': tasks.filter(t => t.status === 'Ready'),
    }

    return (
        <div style={{ padding: 24, height: 'calc(100vh - 80px)' }}>
            <h2 style={{ marginBottom: 20 }}>Kitchen Dashboard 👨‍🍳</h2>
            <div style={{ display: 'grid', gridTemplateColumns: 'repeat(3, 1fr)', gap: 20, height: '100%' }}>
                {Object.entries(columns).map(([status, items]) => (
                    <div key={status} style={{ background: '#f5f5f5', padding: 16, borderRadius: 8, display: 'flex', flexDirection: 'column' }}>
                        <h3 style={{ textAlign: 'center', borderBottom: '2px solid #ccc', paddingBottom: 10 }}>{status} ({items.length})</h3>
                        <div style={{ overflowY: 'auto', flex: 1 }}>
                            {items.map(task => (
                                <div key={task.id} style={{ background: 'white', padding: 12, marginBottom: 12, borderRadius: 6, boxShadow: '0 1px 3px rgba(0,0,0,0.1)' }}>
                                    <div style={{ fontWeight: 'bold' }}>Comanda: {task.orderId.slice(0, 8)}</div>
                                    {task.notes && <div style={{ color: 'red', fontSize: '0.9em', margin: '4px 0' }}>Notă: {task.notes}</div>}
                                    <div style={{ fontSize: '0.8em', color: '#888', marginBottom: 8 }}>
                                        {new Date(task.updatedAt).toLocaleTimeString()}
                                    </div>
                                    <button 
                                        onClick={() => advanceStatus(task)}
                                        style={{ width: '100%', background: '#007bff', color: 'white', border: 'none', padding: 8, borderRadius: 4, cursor: 'pointer', fontWeight: 'bold' }}
                                    >
                                        Avansează &rarr;
                                    </button>
                                </div>
                            ))}
                        </div>
                    </div>
                ))}
            </div>
        </div>
    )
}