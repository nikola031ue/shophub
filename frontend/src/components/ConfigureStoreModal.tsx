import { useState, type FormEvent } from 'react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { updateStore, AvailabilityOptions } from '../api/stores'
import type { Store, StoreAvailability, StoreStatus } from '../types'

interface Props {
  store: Store
  onClose: () => void
}

const STATUS_LABEL: Record<StoreStatus, { label: string; className: string }> = {
  Pending:  { label: 'Kreiranje',  className: 'bg-yellow-100 text-yellow-800' },
  Running:  { label: 'Pokrenuta',  className: 'bg-green-100  text-green-800'  },
  Failed:   { label: 'Greška',     className: 'bg-red-100    text-red-800'    },
  Deleting: { label: 'Brisanje',   className: 'bg-orange-100 text-orange-800' },
}

export function ConfigureStoreModal({ store, onClose }: Props) {
  const queryClient = useQueryClient()
  const [availability, setAvailability] = useState<StoreAvailability>(store.availability)
  const [walletAddress, setWalletAddress] = useState(store.walletAddress)
  const [walletError, setWalletError] = useState('')
  const [submitError, setSubmitError] = useState('')

  const { label, className } = STATUS_LABEL[store.status]

  const mutation = useMutation({
    mutationFn: () => updateStore(store.id, { availability, walletAddress: walletAddress.trim() }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['stores'] })
      onClose()
    },
    onError: () => setSubmitError('Greška pri čuvanju izmena. Pokušaj ponovo.'),
  })

  function handleSubmit(e: FormEvent) {
    e.preventDefault()
    if (!walletAddress.trim()) {
      setWalletError('Wallet adresa je obavezna.')
      return
    }
    setWalletError('')
    setSubmitError('')
    mutation.mutate()
  }

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4"
      onClick={(e) => e.target === e.currentTarget && onClose()}
    >
      <div className="bg-white rounded-2xl shadow-xl w-full max-w-md">
        <div className="flex items-center justify-between px-6 pt-6 pb-4 border-b border-gray-100">
          <h2 className="text-base font-semibold text-gray-900 truncate pr-4">{store.name}</h2>
          <button onClick={onClose} className="text-gray-400 hover:text-gray-600 text-xl leading-none shrink-0">×</button>
        </div>

        {/* Status i URL */}
        <div className="px-6 py-4 bg-gray-50 border-b border-gray-100 space-y-2">
          <div className="flex items-center gap-2">
            <span className="text-sm text-gray-500">Status:</span>
            <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${className}`}>
              {label}
            </span>
          </div>
          {store.url && (
            <div className="flex items-center gap-2 min-w-0">
              <span className="text-sm text-gray-500 shrink-0">URL:</span>
              <a
                href={store.url}
                target="_blank"
                rel="noopener noreferrer"
                className="text-sm text-indigo-600 hover:underline truncate"
              >
                {store.url}
              </a>
            </div>
          )}
        </div>

        <form onSubmit={handleSubmit} className="px-6 py-5 space-y-5">
          {/* Dostupnost */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">Dostupnost</label>
            <div className="space-y-2">
              {AvailabilityOptions.map((opt) => (
                <label
                  key={opt.value}
                  className={`flex items-center gap-3 p-3 border rounded-xl cursor-pointer transition-colors ${
                    availability === opt.value
                      ? 'border-indigo-400 bg-indigo-50'
                      : 'border-gray-200 hover:border-gray-300'
                  }`}
                >
                  <input
                    type="radio"
                    name="availability"
                    value={opt.value}
                    checked={availability === opt.value}
                    onChange={() => setAvailability(opt.value)}
                    className="accent-indigo-600"
                  />
                  <span className="text-sm text-gray-800">{opt.label}</span>
                </label>
              ))}
            </div>
          </div>

          {/* Wallet adresa */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Wallet adresa</label>
            <input
              type="text"
              value={walletAddress}
              onChange={(e) => setWalletAddress(e.target.value)}
              placeholder="0x..."
              className={`w-full px-3 py-2.5 border rounded-xl text-sm font-mono focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent ${
                walletError ? 'border-red-300' : 'border-gray-200'
              }`}
            />
            {walletError && <p className="mt-1 text-xs text-red-500">{walletError}</p>}
          </div>

          {submitError && (
            <p className="text-sm text-red-500 bg-red-50 rounded-lg px-3 py-2">{submitError}</p>
          )}

          <div className="flex gap-3 pt-1">
            <button
              type="button"
              onClick={onClose}
              className="flex-1 py-2.5 border border-gray-200 text-gray-700 text-sm font-medium rounded-xl hover:bg-gray-50 transition-colors"
            >
              Otkaži
            </button>
            <button
              type="submit"
              disabled={mutation.isPending}
              className="flex-1 py-2.5 bg-indigo-600 text-white text-sm font-medium rounded-xl hover:bg-indigo-700 disabled:opacity-60 transition-colors"
            >
              {mutation.isPending ? 'Čuvanje...' : 'Sačuvaj'}
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}
