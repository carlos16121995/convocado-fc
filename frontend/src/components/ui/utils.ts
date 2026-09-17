export function classes(...values: Array<string | undefined | false>) {
  return values.filter(Boolean).join(' ')
}
