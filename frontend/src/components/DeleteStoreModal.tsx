import { useState, type FormEvent } from 'react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { deleteStore } from '../api/stores'
import type { Store } from '../types'

interface Props {
  store: Store
  onClose: () => void
}

export function DeleteStoreModal({ store, onClose }: Props) {
  const queryClient = useQueryClient()
  const [confirmation, setConfirmation] = useState('')
  const [submitError, setSubmitError] = useState('')

  const isConfirmed = confirmation === store.name

  const mutation = useMutation({
    mutationFn: () => deleteStore(store.id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['stores'] })
      onClose()
    },
    onError: () => setSubmitError('Greška pri brisanju prodavnice. Pokušaj ponovo.'),
  })

  function handleSubmit(e: FormEvent) {
    e.preventDefault()
    if (!isConfirmed) return
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
          <h2 className="text-base font-semibold text-gray-900">Obriši prodavnicu</h2>
          <button onClick={onClose} className="text-gray-400 hover:text-gray-600 text-xl leading-none">×</button>
        </div>

        <div className="px-6 py-4 bg-red-50 border-b border-red-100">
          <div className="flex gap-3">
            <span className="text-red-500 text-lg shrink-0">⚠️</span>
            <div className="text-sm text-red-700 space-y-1">
              <p className="font-medium">Ova akcija je nepovratna.</p>
              <p>Brisanjem prodavnice <strong>{store.name}</strong> biće trajno obrisani svi Kubernetes resursi, baza podataka i konfiguracija vezana za ovu prodavnicu.</p>
            </div>
          </div>
        </div>

        <form onSubmit={handleSubmit} className="px-6 py-5 space-y-4">
          <div>
            <label className="block text-sm text-gray-700 mb-1">
              Unesite naziv prodavnice <strong>{store.name}</strong> da potvrdite brisanje:
            </label>
            <input
              type="text"
              value={confirmation}
              onChange={(e) => setConfirmation(e.target.value)}
              autoFocus
              placeholder={store.name}
              className="w-full px-3 py-2.5 border border-gray-200 rounded-xl text-sm focus:outline-none focus:ring-2 focus:ring-red-400 focus:border-transparent"
            />
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
              disabled={!isConfirmed || mutation.isPending}
              className="flex-1 py-2.5 bg-red-600 text-white text-sm font-medium rounded-xl hover:bg-red-700 disabled:opacity-40 disabled:cursor-not-allowed transition-colors"
            >
              {mutation.isPending ? 'Brisanje...' : 'Obriši prodavnicu'}
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}
