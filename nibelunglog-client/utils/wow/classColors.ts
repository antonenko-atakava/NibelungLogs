export const classColors: Record<string, string> = {
  "Паладин": "#F58CBA",
  "Друид": "#FF7D0A",
  "Рыцарь смерти": "#C41F3B",
  "Воин": "#C79C6E",
  "Маг": "#69CCF0",
  "Шаман": "#0070DE",
  "Жрец": "#FFFFFF",
  "Разбойник": "#FFF569",
  "Охотник": "#ABD473",
  "Чернокнижник": "#9482C9",
};

export const classColorsDark: Record<string, string> = {
  "Паладин": "#D67AB8",
  "Друид": "#E66D00",
  "Рыцарь смерти": "#A41A2F",
  "Воин": "#A87C5A",
  "Маг": "#4FB0D0",
  "Шаман": "#0058B0",
  "Жрец": "#D0D0D0",
  "Разбойник": "#D9D04F",
  "Охотник": "#8BB053",
  "Чернокнижник": "#7A6AA5",
};

export function getClassColor(className: string | null | undefined, dark = false): string {
  if (!className)
    return dark ? "#98989d" : "#b0b0b0";
  
  const colorMap = dark ? classColorsDark : classColors;
  return colorMap[className] || (dark ? "#98989d" : "#b0b0b0");
}

export function getClassColorWithOpacity(className: string | null | undefined, opacity = 0.2): string {
  const color = getClassColor(className);
  const hex = color.replace("#", "");
  const r = parseInt(hex.substring(0, 2), 16);
  const g = parseInt(hex.substring(2, 4), 16);
  const b = parseInt(hex.substring(4, 6), 16);
  return `rgba(${r}, ${g}, ${b}, ${opacity})`;
}

