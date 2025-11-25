import { useEffect, useState } from 'react'
import { LoyaltyApi } from '../services/api'
import { LoyaltyTransactionDto } from '../types'
import { Gift, TrendingUp, TrendingDown, Calendar, History } from 'lucide-react'

export default function LoyaltyPage() {
    const [points, setPoints] = useState<number>(0)
    const [transactions, setTransactions] = useState<LoyaltyTransactionDto[]>([])
    const [loading, setLoading] = useState(true)

    useEffect(() => {
        const loadData = async () => {
            try {
                const [accountData, transactionsData] = await Promise.all([
                    LoyaltyApi.getAccount(),
                    LoyaltyApi.getTransactions()
                ])
                setPoints(accountData.points)
                setTransactions(transactionsData)
            } catch (err) {
                console.error(err)
            } finally {
                setLoading(false)
            }
        }
        loadData()
    }, [])

    if (loading) return <div className="text-center py-20 text-gray-500">Se încarcă datele...</div>

    return (
        <div className="max-w-4xl mx-auto">
            <h2 className="text-2xl font-bold mb-6 text-gray-900 flex items-center gap-2">
                <Gift className="text-brand-600" /> Program de Loialitate
            </h2>

            {/* Card Principal Puncte */}
            <div className="bg-gradient-to-r from-brand-600 to-brand-500 rounded-2xl p-8 text-white shadow-xl mb-10 flex flex-col md:flex-row items-center justify-between relative overflow-hidden">
                <div className="relative z-10">
                    <p className="text-brand-100 font-medium mb-1 text-lg">Balanța ta curentă</p>
                    <h3 className="text-5xl font-extrabold">{points} <span className="text-2xl font-normal opacity-80">puncte</span></h3>
                    <p className="mt-4 text-sm bg-white/20 backdrop-blur-sm inline-block px-3 py-1 rounded-full">
                        10 RON cheltuiți = 1 punct câștigat
                    </p>
                </div>
                <div className="relative z-10 mt-6 md:mt-0 bg-white/10 backdrop-blur-md p-4 rounded-xl border border-white/20">
                    <Gift size={64} className="text-white opacity-90" />
                </div>
                
                {/* Elemente decorative */}
                <div className="absolute top-0 right-0 -mr-10 -mt-10 w-40 h-40 bg-white opacity-10 rounded-full blur-2xl"></div>
                <div className="absolute bottom-0 left-0 -ml-10 -mb-10 w-40 h-40 bg-black opacity-10 rounded-full blur-2xl"></div>
            </div>

            {/* Istoric Tranzacții */}
            <div className="bg-white rounded-2xl shadow-sm border border-gray-100 overflow-hidden">
                <div className="p-6 border-b border-gray-100 bg-gray-50/50 flex items-center gap-2">
                    <History className="text-gray-500" size={20}/>
                    <h3 className="font-bold text-gray-800">Istoric Tranzacții</h3>
                </div>

                <div className="divide-y divide-gray-100">
                    {transactions.length === 0 ? (
                        <div className="p-10 text-center text-gray-500 italic">
                            Nu există tranzacții recente.
                        </div>
                    ) : (
                        transactions.map((t) => {
                            const isPositive = t.pointsChange > 0;
                            return (
                                <div key={t.id} className="p-5 hover:bg-gray-50 transition-colors flex items-center justify-between">
                                    <div className="flex items-start gap-4">
                                        <div className={`p-2.5 rounded-full ${isPositive ? 'bg-green-100 text-green-600' : 'bg-red-100 text-red-600'}`}>
                                            {isPositive ? <TrendingUp size={20} /> : <TrendingDown size={20} />}
                                        </div>
                                        <div>
                                            <p className="font-bold text-gray-900">{t.description}</p>
                                            <p className="text-xs text-gray-500 flex items-center gap-1 mt-1">
                                                <Calendar size={12} />
                                                {new Date(t.createdAtUtc).toLocaleString('ro-RO')}
                                            </p>
                                        </div>
                                    </div>
                                    <div className={`text-lg font-bold ${isPositive ? 'text-green-600' : 'text-red-600'}`}>
                                        {isPositive ? '+' : ''}{t.pointsChange} pct
                                    </div>
                                </div>
                            )
                        })
                    )}
                </div>
            </div>
        </div>
    )
}