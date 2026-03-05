"use client";

interface GuildIconProps {
  guildName: string;
  size?: number;
  className?: string;
}

export function GuildIcon({ guildName, size = 24, className }: GuildIconProps) {
  const firstLetter = guildName.charAt(0).toUpperCase();
  const color = getGuildColor(guildName);

  return (
    <svg
      width={size}
      height={size}
      viewBox="0 0 24 24"
      fill="none"
      xmlns="http://www.w3.org/2000/svg"
      className={className}
    >
      <rect
        width="24"
        height="24"
        rx="6"
        fill={`url(#gradient-${guildName.replace(/\s+/g, "-")})`}
        opacity="0.15"
      />
      <rect
        width="24"
        height="24"
        rx="6"
        fill="none"
        stroke={color}
        strokeWidth="1.5"
        opacity="0.3"
      />
      <text
        x="12"
        y="16"
        textAnchor="middle"
        fontSize="12"
        fontWeight="bold"
        fill={color}
        className="select-none"
      >
        {firstLetter}
      </text>
      <defs>
        <linearGradient
          id={`gradient-${guildName.replace(/\s+/g, "-")}`}
          x1="0"
          y1="0"
          x2="24"
          y2="24"
        >
          <stop offset="0%" stopColor={color} stopOpacity="0.25" />
          <stop offset="100%" stopColor={color} stopOpacity="0.1" />
        </linearGradient>
      </defs>
    </svg>
  );
}

function getGuildColor(guildName: string): string {
  const colors = [
    "#94a3b8",
    "#a78bfa",
    "#60a5fa",
    "#34d399",
    "#fbbf24",
    "#fb7185",
    "#818cf8",
    "#f472b6",
  ];

  let hash = 0;
  for (let i = 0; i < guildName.length; i++) {
    hash = guildName.charCodeAt(i) + ((hash << 5) - hash);
  }

  return colors[Math.abs(hash) % colors.length];
}
