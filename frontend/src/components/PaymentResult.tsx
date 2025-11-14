import React, { useEffect, useState } from 'react'

type Props = { onSuccess?: () => void }

export default function PaymentResult({ onSuccess }: Props) {
    const [open, setOpen] = useState(false)
    const [message, setMessage] = useState<string | null>(null)

    useEffect(() => {
        const params = new URLSearchParams(window.location.search)
        const status = params.get('status')
        const sessionId = params.get('session_id')

        if (status === 'success' && sessionId) {
            setMessage('Payment successful')
            setOpen(true)
        } else if (status === 'cancel') {
            setMessage('Payment cancelled')
            setOpen(true)
        }
    }, [])

    function close() {
        // clear query params without reloading
        window.history.replaceState(null, '', window.location.pathname + window.location.hash)
        if (message === 'Payment successful' && onSuccess) onSuccess()
        setOpen(false)
        setMessage(null)
    }

    if (!open || !message) return null

    return (
        <div style={{
            position: 'fixed',
            inset: 0,
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            background: 'rgba(0,0,0,0.4)',
            zIndex: 1000
        }}>
            <div style={{
                background: 'white',
                padding: 24,
                borderRadius: 8,
                maxWidth: 420,
                width: '90%',
                textAlign: 'center',
                boxShadow: '0 8px 24px rgba(0,0,0,0.2)'
            }}>
                <h3 style={{ marginTop: 0 }}>{message}</h3>
                <button onClick={close} style={{ marginTop: 12, padding: '8px 16px' }}>
                    Close
                </button>
            </div>
        </div>
    )
}