import { useState, type FormEvent } from 'react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { createStore, AvailabilityOptions, DatabaseTypeOptions } from '../api/stores'
import type { StoreAvailability, DatabaseType } from '../types'

interface Props {
  onClose: () => void
}

interface FormState {
  name: string
  availability: StoreAvailability
  walletAddress: string
  databaseType: DatabaseType
}

interface FormErrors {
  name?: string
  walletAddress?: string
}

function validate(form: FormState): FormErrors {
  const errors: FormErrors = {}
  if (form.name.trim().length < 2) errors.name = 'Naziv mora imati najmanje 2 karaktera.'
  if (!form.walletAddress.trim()) errors.walletAddress = 'Wallet adresa je obavezna.'
  return errors
}

export function CreateStoreModal({ onClose }: Props) {
  const queryClient = useQueryClient()
  const [form, setForm] = useState<FormState>({
    name: '',
    availability: AvailabilityOptions[0].value,
    walletAddress: '',
    databaseType: DatabaseTypeOptions[0].value,
  })
  const [errors, setErrors] = useState<FormErrors>({})
  const [submitError, setSubmitError] = useState('')

  const mutation = useMutation({
    mutationFn: createStore,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['stores'] })
      onClose()
    },
    onError: () => setSubmitError('Greška pri kreiranju prodavnice. Pokušaj ponovo.'),
  })

  function handleSubmit(e: FormEvent) {
    e.preventDefault()
    const errs = validate(form)
    setErrors(errs)
    if (Object.keys(errs).length > 0) return
    setSubmitError('')
    mutation.mutate({
      name: form.name.trim(),
      availability: form.availability,
      walletAddress: form.walletAddress.trim(),
      databaseType: form.databaseType,
    })
  }

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4"
      onClick={(e) => e.target === e.currentTarget && onClose()}
    >
      <div className="bg-white rounded-2xl shadow-xl w-full max-w-md">
        <div className="flex items-center justify-between px-6 pt-6 pb-4 border-b border-gray-100">
          <h2 className="text-base font-semibold text-gray-900">Nova prodavnica</h2>
          <button onClick={onClose} className="text-gray-400 hover:text-gray-600 text-xl leading-none">×</button>
        </div>

        <form onSubmit={handleSubmit} className="px-6 py-5 space-y-5">
          {/* Naziv */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Naziv prodavnice</label>
            <input
              type="text"
              value={form.name}
              onChange={(e) => setForm({ ...form, name: e.target.value })}
              autoFocus
              placeholder="npr. Prodavnica odeće"
              className={`w-full px-3 py-2.5 border rounded-xl text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent ${
                errors.name ? 'border-red-300' : 'border-gray-200'
              }`}
            />
            {errors.name && <p className="mt-1 text-xs text-red-500">{errors.name}</p>}
          </div>

          {/* Dostupnost */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">Dostupnost</label>
            <div className="space-y-2">
              {AvailabilityOptions.map((opt) => (
                <label
                  key={opt.value}
                  className={`flex items-center gap-3 p-3 border rounded-xl cursor-pointer transition-colors ${
                    form.availability === opt.value
                      ? 'border-indigo-400 bg-indigo-50'
                      : 'border-gray-200 hover:border-gray-300'
                  }`}
                >
                  <input
                    type="radio"
                    name="availability"
                    value={opt.value}
                    checked={form.availability === opt.value}
                    onChange={() => setForm({ ...form, availability: opt.value })}
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
              value={form.walletAddress}
              onChange={(e) => setForm({ ...form, walletAddress: e.target.value })}
              placeholder="0x..."
              className={`w-full px-3 py-2.5 border rounded-xl text-sm font-mono focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent ${
                errors.walletAddress ? 'border-red-300' : 'border-gray-200'
              }`}
            />
            {errors.walletAddress && <p className="mt-1 text-xs text-red-500">{errors.walletAddress}</p>}
          </div>

          {/* Tip baze */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">Tip baze podataka</label>
            <div className="space-y-2">
              {DatabaseTypeOptions.map((opt) => (
                <label
                  key={opt.value}
                  className={`flex items-center gap-3 p-3 border rounded-xl cursor-pointer transition-colors ${
                    form.databaseType === opt.value
                      ? 'border-indigo-400 bg-indigo-50'
                      : 'border-gray-200 hover:border-gray-300'
                  }`}
                >
                  <input
                    type="radio"
                    name="databaseType"
                    value={opt.value}
                    checked={form.databaseType === opt.value}
                    onChange={() => setForm({ ...form, databaseType: opt.value })}
                    className="accent-indigo-600"
                  />
                  <span className="text-sm text-gray-800">{opt.label}</span>
                </label>
              ))}
            </div>
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
              {mutation.isPending ? 'Kreiranje...' : 'Kreiraj'}
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}
