import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { getStores } from '../api/stores'
import { useAuth } from '../context/AuthContext'
import { CreateStoreModal } from '../components/CreateStoreModal'
import { ConfigureStoreModal } from '../components/ConfigureStoreModal'
import { DeleteStoreModal } from '../components/DeleteStoreModal'
import type { Store, StoreStatus } from '../types'

const STATUS_CONFIG: Record<StoreStatus, { label: string; className: string }> = {
  Pending:  { label: 'Kreiranje',  className: 'bg-yellow-100 text-yellow-800' },
  Running:  { label: 'Pokrenuta',  className: 'bg-green-100  text-green-800'  },
  Failed:   { label: 'Greška',     className: 'bg-red-100    text-red-800'    },
  Deleting: { label: 'Brisanje',   className: 'bg-orange-100 text-orange-800' },
}

function StatusBadge({ status }: { status: StoreStatus }) {
  const { label, className } = STATUS_CONFIG[status]
  return (
    <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${className}`}>
      {label}
    </span>
  )
}

function StoreCard({
  store,
  onDelete,
  onConfigure,
}: {
  store: Store
  onDelete: (store: Store) => void
  onConfigure: (store: Store) => void
}) {
  const canOpen = store.status === 'Running' && store.url

  return (
    <div className="bg-white border border-gray-100 rounded-2xl shadow-sm p-6 flex flex-col gap-4">
      <div className="flex items-start justify-between gap-2">
        <h3 className="text-base font-semibold text-gray-900 truncate">{store.name}</h3>
        <StatusBadge status={store.status} />
      </div>

      <dl className="grid grid-cols-2 gap-x-4 gap-y-1 text-sm">
        <dt className="text-gray-400">Dostupnost</dt>
        <dd className="text-gray-700 font-medium">{store.availability}</dd>
        <dt className="text-gray-400">Baza</dt>
        <dd className="text-gray-700 font-medium">{store.databaseType}</dd>
        <dt className="text-gray-400">Wallet</dt>
        <dd className="text-gray-600 font-mono text-xs truncate col-span-1">
          {store.walletAddress.slice(0, 10)}…
        </dd>
      </dl>

      <div className="flex gap-2 mt-auto pt-2 border-t border-gray-50">
        <button
          onClick={() => canOpen && window.open(store.url!, '_blank')}
          disabled={!canOpen}
          className="flex-1 py-1.5 text-sm font-medium rounded-lg bg-indigo-50 text-indigo-700 hover:bg-indigo-100 disabled:opacity-40 disabled:cursor-not-allowed transition-colors"
        >
          Otvori
        </button>
        <button
          onClick={() => onConfigure(store)}
          disabled={store.status === 'Deleting'}
          className="flex-1 py-1.5 text-sm font-medium rounded-lg bg-gray-50 text-gray-700 hover:bg-gray-100 disabled:opacity-40 disabled:cursor-not-allowed transition-colors"
        >
          Konfiguriši
        </button>
        <button
          onClick={() => onDelete(store)}
          disabled={store.status === 'Deleting'}
          className="flex-1 py-1.5 text-sm font-medium rounded-lg bg-red-50 text-red-700 hover:bg-red-100 disabled:opacity-40 disabled:cursor-not-allowed transition-colors"
        >
          Obriši
        </button>
      </div>
    </div>
  )
}

function EmptyState({ onCreate }: { onCreate: () => void }) {
  return (
    <div className="flex flex-col items-center justify-center py-24 text-center">
      <div className="text-5xl mb-4">🏪</div>
      <h3 className="text-lg font-semibold text-gray-900 mb-1">Nema prodavnica</h3>
      <p className="text-sm text-gray-500 mb-6">Kreiraj svoju prvu prodavnicu i pokreni je u par klikova.</p>
      <button
        onClick={onCreate}
        className="px-5 py-2.5 bg-indigo-600 text-white font-medium rounded-xl hover:bg-indigo-700 transition-colors text-sm"
      >
        Kreiraj prodavnicu
      </button>
    </div>
  )
}

export function DashboardPage() {
  const { logout } = useAuth()
  const [showCreateModal, setShowCreateModal] = useState(false)
  const [storeToConfig, setStoreToConfig] = useState<Store | null>(null)
  const [storeToDelete, setStoreToDelete] = useState<Store | null>(null)

  const { data: stores = [], isLoading, isError } = useQuery({
    queryKey: ['stores'],
    queryFn: getStores,
  })

  return (
    <div className="min-h-screen bg-gray-50">
      {showCreateModal && <CreateStoreModal onClose={() => setShowCreateModal(false)} />}
      {storeToConfig && <ConfigureStoreModal store={storeToConfig} onClose={() => setStoreToConfig(null)} />}
      {storeToDelete && <DeleteStoreModal store={storeToDelete} onClose={() => setStoreToDelete(null)} />}

      <header className="bg-white border-b border-gray-100 px-6 py-4 flex items-center justify-between">
        <h1 className="text-lg font-bold text-gray-900">ShopHub</h1>
        <div className="flex items-center gap-3">
          {stores.length > 0 && (
            <button
              onClick={() => setShowCreateModal(true)}
              className="px-4 py-2 bg-indigo-600 text-white text-sm font-medium rounded-xl hover:bg-indigo-700 transition-colors"
            >
              + Kreiraj prodavnicu
            </button>
          )}
          <button
            onClick={logout}
            className="px-4 py-2 text-sm font-medium text-gray-600 hover:text-gray-900 transition-colors"
          >
            Odjavi se
          </button>
        </div>
      </header>

      <main className="max-w-6xl mx-auto px-6 py-8">
        <div className="mb-6">
          <h2 className="text-xl font-semibold text-gray-900">Moje prodavnice</h2>
          {stores.length > 0 && (
            <p className="text-sm text-gray-500 mt-0.5">{stores.length} prodavnica</p>
          )}
        </div>

        {isLoading && (
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
            {[1, 2, 3].map((i) => (
              <div key={i} className="bg-white border border-gray-100 rounded-2xl h-48 animate-pulse" />
            ))}
          </div>
        )}

        {isError && (
          <div className="bg-red-50 border border-red-100 rounded-xl px-4 py-3 text-sm text-red-700">
            Greška pri učitavanju prodavnica. Pokušaj ponovo.
          </div>
        )}

        {!isLoading && !isError && stores.length === 0 && (
          <EmptyState onCreate={() => setShowCreateModal(true)} />
        )}

        {!isLoading && !isError && stores.length > 0 && (
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
            {stores.map((store) => (
              <StoreCard
                key={store.id}
                store={store}
                onDelete={setStoreToDelete}
                onConfigure={setStoreToConfig}
              />
            ))}
          </div>
        )}
      </main>
    </div>
  )
}
