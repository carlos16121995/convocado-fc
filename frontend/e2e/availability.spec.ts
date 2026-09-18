import { expect, test } from '@playwright/test'

test('mostra a disponibilidade consultando a API real', async ({ page }) => {
  await page.goto('/')

  await expect(page.getByText('Disponível')).toBeVisible()
  await expect(page.getByText('A API está conectada à base de dados.')).toBeVisible()
})
