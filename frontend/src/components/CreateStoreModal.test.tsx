import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { CreateStoreModal } from './CreateStoreModal'
import * as storesApi from '../api/stores'

vi.mock('../api/stores', async (importOriginal) => {
  const mod = await importOriginal<typeof import('../api/stores')>()
  return { ...mod, createStore: vi.fn() }
})

const onClose = vi.fn()

function renderModal() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return render(
    <QueryClientProvider client={queryClient}>
      <CreateStoreModal onClose={onClose} />
    </QueryClientProvider>,
  )
}

describe('CreateStoreModal', () => {
  beforeEach(() => {
    vi.restoreAllMocks()
    onClose.mockReset()
    vi.mocked(storesApi.createStore).mockResolvedValue({ id: 'new-id' })
  })

  it('renders all form fields', () => {
    renderModal()
    expect(screen.getByPlaceholderText(/npr\. Prodavnica/)).toBeInTheDocument()
    expect(screen.getByPlaceholderText('0x...')).toBeInTheDocument()
    expect(screen.getByLabelText(/Standard \(2 replike\)/)).toBeInTheDocument()
    expect(screen.getByLabelText(/High \(3 replike\)/)).toBeInTheDocument()
    expect(screen.getByLabelText(/Standard \(PostgreSQL\)/)).toBeInTheDocument()
    expect(screen.getByLabelText(/Light \(Redis\)/)).toBeInTheDocument()
  })

  it('shows validation errors when form is empty', async () => {
    renderModal()
    await userEvent.click(screen.getByRole('button', { name: 'Kreiraj' }))
    expect(screen.getByText('Naziv mora imati najmanje 2 karaktera.')).toBeInTheDocument()
    expect(screen.getByText('Wallet adresa je obavezna.')).toBeInTheDocument()
    expect(storesApi.createStore).not.toHaveBeenCalled()
  })

  it('shows validation error when name is too short', async () => {
    renderModal()
    await userEvent.type(screen.getByPlaceholderText(/npr\. Prodavnica/), 'A')
    await userEvent.type(screen.getByPlaceholderText('0x...'), '0xabc')
    await userEvent.click(screen.getByRole('button', { name: 'Kreiraj' }))
    expect(screen.getByText('Naziv mora imati najmanje 2 karaktera.')).toBeInTheDocument()
  })

  it('submits with correct payload and closes modal', async () => {
    renderModal()
    await userEvent.type(screen.getByPlaceholderText(/npr\. Prodavnica/), 'Moja Prodavnica')
    await userEvent.type(screen.getByPlaceholderText('0x...'), '0xabc123')
    await userEvent.click(screen.getByLabelText(/High \(3 replike\)/))
    await userEvent.click(screen.getByLabelText(/Light \(Redis\)/))
    await userEvent.click(screen.getByRole('button', { name: 'Kreiraj' }))

    await waitFor(() => {
      expect(storesApi.createStore).toHaveBeenCalledOnce()
      const payload = vi.mocked(storesApi.createStore).mock.calls[0][0]
      expect(payload).toEqual({ name: 'Moja Prodavnica', availability: 'High', walletAddress: '0xabc123', databaseType: 'Light' })
    })
    await waitFor(() => expect(onClose).toHaveBeenCalled())
  })

  it('calls onClose when Otkaži is clicked', async () => {
    renderModal()
    await userEvent.click(screen.getByRole('button', { name: 'Otkaži' }))
    expect(onClose).toHaveBeenCalled()
  })

  it('calls onClose when clicking backdrop', async () => {
    renderModal()
    await userEvent.click(document.querySelector('.fixed.inset-0')!)
    expect(onClose).toHaveBeenCalled()
  })
})
