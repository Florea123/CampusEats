import { useEffect, useState } from 'react'
import { OrderApi } from '../services/api'
import { OrderDto, OrderStatus } from '../types'

export default function OrdersPage() {
    const [orders, setOrders] = useState<OrderDto[]>([])
    const [loading, setLoading] = useState(true)

    useEffect(() => {
        loadOrders()
    }, [])

    const loadOrders = async () => {
        try {
            const data = await OrderApi.getAll()
            setOrders(data)
        } catch (err) {
            console.error(err)
        } finally {
            setLoading(false)
        }
    }

    const handleCancel = async (id: string) => {
        if (!confirm('Sigur dorești să anulezi comanda?')) return
        try {
            await OrderApi.cancel(id)
            await loadOrders()
        } catch (err) {
            alert('Nu s-a putut anula comanda (posibil să fie deja în preparare).')
        }
    }

    if (loading) return <div style={{ padding: 24 }}>Se încarcă comenzile...</div>

    return (
        <div style={{ maxWidth: 800, margin: '0 auto', padding: 24 }}>
            <h2>Comenzile Mele</h2>
            {orders.length === 0 ? <p>Nu ai plasat nicio comandă.</p> : (
                <div style={{ display: 'flex', flexDirection: 'column', gap: 16 }}>
                    {orders.map(order => (
                        <div key={order.id} style={{ border: '1px solid #ddd', padding: 16, borderRadius: 8, backgroundColor: '#fff' }}>
                            <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 8 }}>
                                <strong>Comanda #{order.id.slice(0, 8)}</strong>
                                <span style={{ 
                                    fontWeight: 'bold',
                                    color: order.status === OrderStatus.Cancelled ? 'red' : 
                                           order.status === OrderStatus.Completed ? 'green' : 'orange' 
                                }}>
                                    {OrderStatus[order.status]}
                                </span>
                            </div>
                            <p style={{ color: '#666', fontSize: '0.9em', margin: '4px 0' }}>
                                {new Date(order.createdAtUtc).toLocaleString()}
                            </p>
                            <ul style={{ margin: '8px 0', paddingLeft: 20 }}>
                                {order.items.map(item => (
                                    <li key={item.id}>
                                        {item.quantity}x {item.menuItemName || 'Produs necunoscut'} 
                                        ({item.unitPrice} RON)
                                    </li>
                                ))}
                            </ul>
                            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginTop: 12, borderTop: '1px solid #eee', paddingTop: 8 }}>
                                <strong>Total: {order.total.toFixed(2)} RON</strong>
                                {order.status === OrderStatus.Pending && (
                                    <button 
                                        onClick={() => handleCancel(order.id)} 
                                        style={{ backgroundColor: '#ff4444', color: 'white', border: 'none', padding: '6px 12px', borderRadius: 4, cursor: 'pointer' }}>
                                        Anulează Comanda
                                    </button>
                                )}
                            </div>
                        </div>
                    ))}
                </div>
            )}
        </div>
    )
}