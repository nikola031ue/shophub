import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { ConfigureStoreModal } from './ConfigureStoreModal'
import * as storesApi from '../api/stores'
import type { Store } from '../types'

vi.mock('../api/stores', async (importOriginal) => {
  const mod = await importOriginal<typeof import('../api/stores')>()
  return { ...mod, updateStore: vi.fn() }
})

const onClose = vi.fn()

function makeStore(overrides: Partial<Store> = {}): Store {
  return {
    id: 'store-1',
    name: 'Test Store',
    availability: 'Standard',
    walletAddress: '0xabc123',
    databaseType: 'Standard',
    status: 'Running',
    url: 'http://test.local',
    createdAt: '2025-01-01T00:00:00Z',
    updatedAt: '2025-01-01T00:00:00Z',
    ...overrides,
  }
}

function renderModal(store = makeStore()) {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return render(
    <QueryClientProvider client={queryClient}>
      <ConfigureStoreModal store={store} onClose={onClose} />
    </QueryClientProvider>,
  )
}

describe('ConfigureStoreModal', () => {
  beforeEach(() => {
    vi.restoreAllMocks()
    onClose.mockReset()
    vi.mocked(storesApi.updateStore).mockResolvedValue(undefined)
  })

  it('shows store name, status badge and URL', () => {
    renderModal()
    expect(screen.getByText('Test Store')).toBeInTheDocument()
    expect(screen.getByText('Pokrenuta')).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'http://test.local' })).toBeInTheDocument()
  })

  it('does not show URL section when url is null', () => {
    renderModal(makeStore({ url: null }))
    expect(screen.queryByText('URL:')).not.toBeInTheDocument()
  })

  it('prefills form with current store values', () => {
    renderModal(makeStore({ availability: 'High', walletAddress: '0xdeadbeef' }))
    expect(screen.getByLabelText(/High \(3 replike\)/)).toBeChecked()
    expect(screen.getByPlaceholderText('0x...')).toHaveValue('0xdeadbeef')
  })

  it('shows error when wallet address is empty', async () => {
    renderModal()
    await userEvent.clear(screen.getByPlaceholderText('0x...'))
    await userEvent.click(screen.getByRole('button', { name: 'Sačuvaj' }))
    expect(screen.getByText('Wallet adresa je obavezna.')).toBeInTheDocument()
    expect(storesApi.updateStore).not.toHaveBeenCalled()
  })

  it('calls updateStore with correct payload and closes on success', async () => {
    renderModal()
    await userEvent.click(screen.getByLabelText(/High \(3 replike\)/))
    await userEvent.clear(screen.getByPlaceholderText('0x...'))
    await userEvent.type(screen.getByPlaceholderText('0x...'), '0xnewwallet')
    await userEvent.click(screen.getByRole('button', { name: 'Sačuvaj' }))

    await waitFor(() => {
      expect(storesApi.updateStore).toHaveBeenCalledOnce()
      const [id, payload] = vi.mocked(storesApi.updateStore).mock.calls[0]
      expect(id).toBe('store-1')
      expect(payload).toEqual({ availability: 'High', walletAddress: '0xnewwallet' })
    })
    await waitFor(() => expect(onClose).toHaveBeenCalled())
  })

  it('calls onClose when Otkaži is clicked', async () => {
    renderModal()
    await userEvent.click(screen.getByRole('button', { name: 'Otkaži' }))
    expect(onClose).toHaveBeenCalled()
  })
})
