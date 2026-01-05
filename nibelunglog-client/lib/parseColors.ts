export function getParseColor(parse?: number): string {
  if (!parse) return '#666666';
  
  if (parse >= 100) return '#e5cc80';
  if (parse >= 99) return '#e268a8';
  if (parse >= 95) return '#ff8000';
  if (parse >= 75) return '#a335ee';
  if (parse >= 50) return '#0070ff';
  if (parse >= 25) return '#1eff00';
  return '#666666';
}

