import { useState } from 'react'
import type { MenuItem } from '../types'
import { PaymentApi } from '../services/api'

type CartItem = { item: MenuItem; quantity: number }

type Props = {
    cart: CartItem[]
    onClear: () => void
    onUpdateQuantity: (itemId: string, qty: number) => void
}

export default function OrderCart({ cart, onClear, onUpdateQuantity }: Props) {
    const [isOpen, setIsOpen] = useState(false)
    const [loading, setLoading] = useState(false)

    const total = cart.reduce((sum, c) => sum + c.item.price * c.quantity, 0)

    const handlePayNow = async () => {
        if (cart.length === 0) return

        setLoading(true)
        try {
            const items = cart.map(c => ({ menuItemId: c.item.id, quantity: c.quantity }))
            const { checkoutUrl } = await PaymentApi.createSession(items)
            window.location.href = checkoutUrl
        } catch (err: any) {
            alert('Payment failed: ' + err.message)
        } finally {
            setLoading(false)
        }
    }

    if (!isOpen) {
        return (
            <button
                onClick={() => setIsOpen(true)}
                style={{
                    position: 'fixed',
                    bottom: 24,
                    right: 24,
                    padding: '12px 24px',
                    fontSize: 16,
                    fontWeight: 'bold',
                    backgroundColor: '#007bff',
                    color: '#fff',
                    border: 'none',
                    borderRadius: 8,
                    cursor: 'pointer',
                }}
            >
                View Order ({cart.length})
            </button>
        )
    }

    return (
        <div
            style={{
                position: 'fixed',
                bottom: 24,
                right: 24,
                width: 400,
                maxHeight: 500,
                backgroundColor: '#fff',
                border: '1px solid #ccc',
                borderRadius: 8,
                boxShadow: '0 4px 12px rgba(0,0,0,0.15)',
                display: 'flex',
                flexDirection: 'column',
            }}
        >
            <div style={{ padding: 16, borderBottom: '1px solid #eee', display: 'flex', justifyContent: 'space-between' }}>
                <h3 style={{ margin: 0 }}>Your Order</h3>
                <button onClick={() => setIsOpen(false)}>×</button>
            </div>

            <div style={{ flex: 1, overflowY: 'auto', padding: 16 }}>
                {cart.length === 0 ? (
                    <p>No items in cart</p>
                ) : (
                    cart.map(c => (
                        <div key={c.item.id} style={{ display: 'flex', gap: 8, marginBottom: 12, alignItems: 'center' }}>
                            <div style={{ flex: 1 }}>
                                <div>{c.item.name}</div>
                                <div style={{ fontSize: 14, color: '#666' }}>${c.item.price.toFixed(2)}</div>
                            </div>
                            <input
                                type="number"
                                min="1"
                                value={c.quantity}
                                onChange={e => onUpdateQuantity(c.item.id, parseInt(e.target.value) || 1)}
                                style={{ width: 60 }}
                            />
                            <button onClick={() => onUpdateQuantity(c.item.id, 0)}>Remove</button>
                        </div>
                    ))
                )}
            </div>

            <div style={{ padding: 16, borderTop: '1px solid #eee' }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 12, fontWeight: 'bold' }}>
                    <span>Total:</span>
                    <span>${total.toFixed(2)}</span>
                </div>
                <button
                    onClick={handlePayNow}
                    disabled={cart.length === 0 || loading}
                    style={{
                        width: '100%',
                        padding: 12,
                        backgroundColor: '#28a745',
                        color: '#fff',
                        border: 'none',
                        borderRadius: 4,
                        fontWeight: 'bold',
                        cursor: cart.length === 0 || loading ? 'not-allowed' : 'pointer',
                    }}
                >
                    {loading ? 'Processing...' : 'Pay Now'}
                </button>
                {cart.length > 0 && (
                    <button
                        onClick={onClear}
                        style={{ width: '100%', padding: 8, marginTop: 8 }}
                    >
                        Clear Cart
                    </button>
                )}
            </div>
        </div>
    )
}
