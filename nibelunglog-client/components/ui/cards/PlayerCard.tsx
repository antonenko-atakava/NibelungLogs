import Link from 'next/link';
import { PlayerDto } from '@/types/api/PlayerDto';

interface PlayerCardProps {
  player: PlayerDto;
}

export default function PlayerCard({ player }: PlayerCardProps) {
  return (
    <Link href={`/players/${player.id}`} className="block bg-white rounded-xl shadow-lg border border-gray-100 p-8 hover:shadow-2xl hover:-translate-y-1 transition-all duration-300 group">
      <div className="flex justify-between items-start mb-6">
        <div>
          <div className="flex items-center gap-3 mb-2">
            <span className="text-xs font-bold text-white bg-gradient-to-r from-gray-800 to-gray-900 px-3 py-1.5 rounded-full shadow-sm">#{player.rank}</span>
            <h3 className="text-2xl font-bold text-gray-900 group-hover:text-gray-700 transition-colors">{player.characterName}</h3>
          </div>
          <p className="text-sm font-medium text-gray-500">{player.characterClass} • {player.characterRace} • Уровень {player.characterLevel}</p>
        </div>
      </div>
      <div className="grid grid-cols-2 gap-6 pt-6 border-t border-gray-100">
        <div>
          <p className="text-xs font-semibold text-gray-400 uppercase tracking-wide mb-1">Средний DPS</p>
          <p className="text-lg font-bold text-gray-900">{Math.round(player.averageDps).toLocaleString()}</p>
        </div>
        <div>
          <p className="text-xs font-semibold text-gray-400 uppercase tracking-wide mb-1">Макс. DPS</p>
          <p className="text-lg font-bold text-gray-900">{Math.round(player.maxDps).toLocaleString()}</p>
        </div>
        <div>
          <p className="text-xs font-semibold text-gray-400 uppercase tracking-wide mb-1">Энкаунтеров</p>
          <p className="text-lg font-bold text-gray-900">{player.totalEncounters}</p>
        </div>
        <div>
          <p className="text-xs font-semibold text-gray-400 uppercase tracking-wide mb-1">Урон</p>
          <p className="text-lg font-bold text-gray-900">{(player.totalDamage / 1000000).toFixed(1)}M</p>
        </div>
      </div>
    </Link>
  );
}

