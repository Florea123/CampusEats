import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { KitchenApi } from '../services/api'
import { KitchenTaskDto } from '../types'
import {
    RefreshCcw,
    Clock,
    CheckCircle,
    ChefHat,
    ArrowRight,
    Flame,
    Trash2,
    AlertTriangle,
} from 'lucide-react'

const columnConfigs = [
    { key: 'NotStarted', title: 'Comenzi Noi',        colorClass: 'bg-red-500',   Icon: Clock },
    { key: 'Preparing',  title: 'În Preparare',       colorClass: 'bg-blue-500',  Icon: ChefHat },
    { key: 'Ready',      title: 'Gata de Ridicare',   colorClass: 'bg-green-500', Icon: CheckCircle },
] as const

type StatusKey = typeof columnConfigs[number]['key']

export default function KitchenDashboard() {
    const [tasks, setTasks] = useState<KitchenTaskDto[]>([])
    const [activeMobileStatus, setActiveMobileStatus] = useState<StatusKey>('NotStarted')
    const navigate = useNavigate()

    const loadTasks = async () => {
        try {
            const data = await KitchenApi.getAll()
            const safeData = Array.isArray(data) ? data : []
            setTasks(
                safeData.sort(
                    (a, b) =>
                        new Date(a.updatedAt).getTime() - new Date(b.updatedAt).getTime()
                )
            )
        } catch (err) {
            console.error('Failed to load tasks', err)
            setTasks([])
        }
    }

    useEffect(() => {
        loadTasks()
        const interval = setInterval(loadTasks, 5000)
        return () => clearInterval(interval)
    }, [])

    const advanceStatus = async (task: KitchenTaskDto) => {
        if (task.orderStatus === 'Cancelled') return

        const statusMap: Record<string, string> = {
            NotStarted: 'Preparing',
            Preparing: 'Ready',
            Ready: 'Completed',
        }

        const nextStatus = statusMap[task.status]
        if (!nextStatus) return

        try {
            await KitchenApi.updateStatus(task.id, nextStatus)
            loadTasks()
        } catch (err: any) {
            alert(err.message || 'Eroare la actualizarea statusului.')
        }
    }

    const handleDismiss = async (id: string) => {
        try {
            await KitchenApi.updateStatus(id, 'Completed')
            loadTasks()
        } catch (err: any) {
            alert(err.message || 'Eroare la eliminare')
        }
    }

    const renderColumn = (
        title: string,
        statusFilter: StatusKey,
        colorClass: string,
        Icon: any
    ) => {
        const items = tasks.filter((t) => t.status === statusFilter)
        return (
            <div className="flex flex-col h-full bg-gray-100/80 rounded-2xl border border-gray-200 overflow-hidden shadow-inner">
                <div className={`p-4 border-b border-gray-200 ${colorClass} bg-opacity-10 flex items-center gap-2 backdrop-blur-sm`}>
                    <Icon className={colorClass.replace('bg-', 'text-')} size={20} />
                    <h3 className="font-bold text-gray-800 tracking-tight">{title}</h3>
                    <span className="ml-auto bg-white px-2.5 py-0.5 rounded-md text-xs font-bold shadow-sm text-gray-600 border border-gray-100">
                        {items.length}
                    </span>
                </div>
                <div className="p-3 overflow-y-auto flex-1 space-y-3 custom-scrollbar">
                    {items.map((task) => {
                        const isCancelled = task.orderStatus === 'Cancelled'

                        return (
                            <div
                                key={task.id}
                                className={`bg-white p-4 rounded-xl shadow-sm border transition-all group ${
                                    isCancelled
                                        ? 'border-red-300 bg-red-50'
                                        : 'border-gray-100 hover:border-l-brand-500 border-l-4 border-l-transparent'
                                }`}
                            >
                                <div className="flex justify-between items-start mb-2"> 
                                    <span className="font-mono font-bold text-lg text-gray-900">
                                        #{task.orderId.slice(0, 4)}
                                    </span>
                                    <span className="text-[10px] font-bold text-gray-400 uppercase bg-white px-2 py-1 rounded border border-gray-100">
                                        {new Date(task.updatedAt).toLocaleTimeString([], {
                                            hour: '2-digit',
                                            minute: '2-digit',
                                        })}
                                     </span>
                                </div>

                                {isCancelled && (
                                    <div className="mb-3 flex items-center gap-2 text-red-700 font-bold text-xs bg-red-100 p-2 rounded-lg border border-red-200">
                                        <AlertTriangle size={14} />
                                        ANULATĂ
                                    </div>
                                )}

                                {task.notes && !isCancelled && (
                                    <div className="bg-yellow-50 text-yellow-800 text-xs font-medium p-2 rounded-lg mb-3 border border-yellow-100 flex gap-1">
                                        <span>📝</span> {task.notes}
                                    </div>
                                )}

                                <div className="flex gap-2 mt-3 pt-3 border-t border-gray-100">
                                    <button
                                        onClick={() => navigate(`/kitchen/order/${task.orderId}`)}
                                        className="flex-1 py-2 bg-white border border-gray-200 text-gray-600 hover:bg-gray-50 hover:text-gray-900 hover:border-gray-300 rounded-lg text-sm font-bold transition-all shadow-sm"
                                    >
                                        Detalii
                                    </button>

                                    {isCancelled ? (
                                        <button
                                            onClick={() => handleDismiss(task.id)}
                                            className="flex-1 py-2 bg-red-600 hover:bg-red-700 text-white rounded-lg text-sm font-bold transition-all flex items-center justify-center gap-2 shadow-sm"
                                            title="Elimină din vizualizare"
                                        >
                                            <Trash2 size={14} /> Elimină
                                        </button>
                                    ) : (
                                        <button
                                            onClick={() => advanceStatus(task)}
                                            className="flex-1 py-2 bg-gray-900 hover:bg-brand-600 text-white rounded-lg text-sm font-bold transition-all flex items-center justify-center gap-2 opacity-0 group-hover:opacity-100 translate-y-2 group-hover:translate-y-0"
                                        >
                                            Avansează <ArrowRight size={14} />
                                        </button>
                                    )}
                                </div>
                            </div>
                        )
                    })}
                    {items.length === 0 && (
                        <div className="text-center py-10 text-gray-400 text-sm italic flex flex-col items-center">
                            <div className="w-12 h-12 bg-gray-200/50 rounded-full flex items-center justify-center mb-2">
                                <CheckCircle size={20} className="opacity-50" />
                            </div>
                            Niciun task
                        </div>
                    )}
                </div>
            </div>
        )
    }

    return (
        <div className="h-[calc(100vh-140px)] flex flex-col">
            <div className="flex justify-between items-center mb-6">
                <div>
                    <h2 className="text-2xl font-bold text-gray-900 flex items-center gap-2">
                        <Flame className="text-orange-500" fill="currentColor" />
                        Kitchen Monitor
                    </h2>
                    <p className="text-gray-500 text-sm">Gestionează comenzile în timp real</p>
                </div>
                <button
                    onClick={loadTasks}
                    className="p-2.5 bg-white border border-gray-200 rounded-xl hover:bg-brand-50 hover:text-brand-600 text-gray-600 shadow-sm transition-all active:scale-95"
                >
                    <RefreshCcw size={20} />
                </button>
            </div>

            {/* Desktop / tablet: 3 columns nicely on the page */}
            <div className="hidden md:grid grid-cols-3 gap-6 h-full">
                {columnConfigs.map((col) => (
                    <div key={col.key} className="h-full">
                        {renderColumn(col.title, col.key, col.colorClass, col.Icon)}
                    </div>
                ))}
            </div>

            {/* Mobile: buttons (tabs) + single column filling the phone width */}
            <div className="md:hidden flex flex-col gap-4 h-full">
                <div className="overflow-x-auto -mx-2 px-2 pb-1">
                    <div className="flex gap-2 w-max">
                        {columnConfigs.map((col) => {
                            const count = tasks.filter((t) => t.status === col.key).length
                            const isActive = activeMobileStatus === col.key

                            return (
                                <button
                                    key={col.key}
                                    onClick={() => setActiveMobileStatus(col.key)}
                                    className={`flex-none flex items-center justify-center gap-1.5 rounded-full px-3 py-2 text-xs font-semibold border transition-colors whitespace-nowrap ${
                                        isActive
                                            ? 'bg-gray-900 text-white border-gray-900'
                                            : 'bg-white text-gray-700 border-gray-200'
                                    }`}
                                >
                                    <col.Icon className="w-4 h-4" />
                                    <span className="truncate max-w-[6rem]">{col.title}</span>
                                    <span
                                        className={`inline-flex items-center justify-center min-w-[1.5rem] h-5 rounded-full text-[10px] font-bold ${
                                            isActive ? 'bg-white text-gray-900' : 'bg-gray-100 text-gray-600'
                                        }`}
                                    >
                                        {count}
                                    </span>
                                </button>
                            )
                        })}
                    </div>
                </div>

                <div className="flex-1">
                    {(() => {
                        const activeConfig = columnConfigs.find(
                            (c) => c.key === activeMobileStatus
                        )!
                        return renderColumn(
                            activeConfig.title,
                            activeConfig.key,
                            activeConfig.colorClass,
                            activeConfig.Icon
                        )
                    })()}
                </div>
            </div>
        </div>
    )
}