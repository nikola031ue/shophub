import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { DeleteStoreModal } from './DeleteStoreModal'
import * as storesApi from '../api/stores'
import type { Store } from '../types'

vi.mock('../api/stores', async (importOriginal) => {
  const mod = await importOriginal<typeof import('../api/stores')>()
  return { ...mod, deleteStore: vi.fn() }
})

const onClose = vi.fn()

const store: Store = {
  id: 'store-1',
  name: 'Moja Prodavnica',
  availability: 'Standard',
  walletAddress: '0xabc',
  databaseType: 'Standard',
  status: 'Running',
  url: null,
  createdAt: '2025-01-01T00:00:00Z',
  updatedAt: '2025-01-01T00:00:00Z',
}

function renderModal() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return render(
    <QueryClientProvider client={queryClient}>
      <DeleteStoreModal store={store} onClose={onClose} />
    </QueryClientProvider>,
  )
}

describe('DeleteStoreModal', () => {
  beforeEach(() => {
    vi.restoreAllMocks()
    onClose.mockReset()
    vi.mocked(storesApi.deleteStore).mockResolvedValue(undefined)
  })

  it('renders warning and store name', () => {
    renderModal()
    expect(screen.getByRole('heading', { name: 'Obriši prodavnicu' })).toBeInTheDocument()
    expect(screen.getByText(/Ova akcija je nepovratna/)).toBeInTheDocument()
    expect(screen.getAllByText('Moja Prodavnica').length).toBeGreaterThan(0)
  })

  it('submit button is disabled until correct name is typed', async () => {
    renderModal()
    const btn = screen.getByRole('button', { name: 'Obriši prodavnicu' })
    expect(btn).toBeDisabled()

    await userEvent.type(screen.getByPlaceholderText('Moja Prodavnica'), 'Moja ')
    expect(btn).toBeDisabled()

    await userEvent.type(screen.getByPlaceholderText('Moja Prodavnica'), 'Prodavnica')
    expect(btn).not.toBeDisabled()
  })

  it('calls deleteStore and closes modal on confirmation', async () => {
    renderModal()
    await userEvent.type(screen.getByPlaceholderText('Moja Prodavnica'), 'Moja Prodavnica')
    await userEvent.click(screen.getByRole('button', { name: 'Obriši prodavnicu' }))

    await waitFor(() => {
      expect(storesApi.deleteStore).toHaveBeenCalledOnce()
      expect(vi.mocked(storesApi.deleteStore).mock.calls[0][0]).toBe('store-1')
    })
    await waitFor(() => expect(onClose).toHaveBeenCalled())
  })

  it('calls onClose when Otkaži is clicked', async () => {
    renderModal()
    await userEvent.click(screen.getByRole('button', { name: 'Otkaži' }))
    expect(onClose).toHaveBeenCalled()
  })
})
